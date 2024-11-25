public class RolesController : Controller
{
    private readonly UserManager<CustomUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly DivarDbContext _context;

    public RolesController(UserManager<CustomUser> userManager, RoleManager<IdentityRole> roleManager, DivarDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context; 
    }



    public IActionResult Index()
    {
        var roles = _roleManager.Roles.ToList();
        return View(roles);
    }



    // ایجاد نقش جدید
    [HttpGet]
    public IActionResult CreateRole()
    {
        var model = new CreateRoleViewModel();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Create the new role
        var role = new Role { Name = model.RoleName };

        // Optional: Add permissions if needed
        if (model.SelectedPermissions != null && model.SelectedPermissions.Any())
        {
            var permissions = model.SelectedPermissions
                .Select(p => (AccessLevel)Enum.Parse(typeof(AccessLevel), p))
                .ToList();

            role.Permissions = permissions;
        }

        var result = await _roleManager.CreateAsync(role);
        if (result.Succeeded)
        {
            // Add RolePermissions after creating the role
            foreach (var permission in role.Permissions)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = role.Id,
                    Permission = permission
                };
                await _context.RolePermissions.AddAsync(rolePermission);
            }

            await _context.SaveChangesAsync(); // save the changes

            return RedirectToAction("Index");
        }

        // If creating the role fails, add errors to the ModelState
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }





    // حذف نقش
    public async Task<IActionResult> DeleteRole(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            // خطاهای ممکن را مدیریت کنید
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View("Index", await _roleManager.Roles.ToListAsync());
        }

        return RedirectToAction("Index");
    }




    //  اختصاص نقش به کاربر
    public IActionResult SelectUser()
    {
        // نمایش لیست کاربران برای انتخاب
        var users = _userManager.Users.ToList();
        return View(users);
    }

    public async Task<IActionResult> ManageRole(string id)
    {
        if (id == null)
        {
            return RedirectToAction("SelectUser");
        }

        // Find the user by Id
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Get user roles and all roles
        var userRoles = await _userManager.GetRolesAsync(user);
        var allRoles = _roleManager.Roles.ToList();

        // Create the ManageRoleViewModel including additional fields
        var model = new ManageRoleViewModel
        {
            CustomUserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            UserRoles = userRoles.ToList(),
            AllRoles = allRoles
        };

        return View(model);
    }


    // مدیریت نقش کاربران (POST)
    [HttpPost]
    public async Task<IActionResult> ManageRole(ManageRoleViewModel model)
    {
        // پیدا کردن کاربر بر اساس Id
        var user = await _userManager.FindByIdAsync(model.CustomUserId);
        if (user == null)
        {
            return NotFound();
        }

        // گرفتن نقش‌های کاربر فعلی
        var userRoles = await _userManager.GetRolesAsync(user);
        var rolesToAdd = model.Roles.Except(userRoles).ToList();
        var rolesToRemove = userRoles.Except(model.Roles).ToList();

        // اضافه کردن نقش‌ها
        foreach (var role in rolesToAdd)
        {
            await _userManager.AddToRoleAsync(user, role);
        }

        // حذف نقش‌ها
        foreach (var role in rolesToRemove)
        {
            await _userManager.RemoveFromRoleAsync(user, role);
        }

        return RedirectToAction("Index");
    }




    //ویرایش نقش ها
    public async Task<IActionResult> EditRole(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return BadRequest("Role ID cannot be null or empty.");
        }

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        // Get the existing permissions for the role
        var existingPermissions = _context.Roles
            .Where(r => r.Id == id)
            .Select(r => r.Permissions)
            .FirstOrDefault(); // Assuming you have only one Role object based on Id

        var model = new CreateRoleViewModel
        {
            RoleName = role.Name,
            SelectedPermissions = existingPermissions?.Select(p => p.ToString()).ToList() ?? new List<string>()
        };

        return View(model);
    }

    // POST: EditRole
    [HttpPost]
    public async Task<IActionResult> EditRole(string id, CreateRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        // Update the role name
        role.Name = model.RoleName;

        // Update permissions
        var permissions = model.SelectedPermissions
            .Select(p => (AccessLevel)Enum.Parse(typeof(AccessLevel), p))
            .ToList();


        var roleEntity = await _context.Roles.FindAsync(id);
        if (roleEntity != null)
        {
            roleEntity.Permissions = permissions; // assuming this is configured correctly in your entity
            _context.Roles.Update(roleEntity);
        }

        var result = await _roleManager.UpdateAsync(role);
        if (result.Succeeded)
        {
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

}


