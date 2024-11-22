using Microsoft.AspNetCore.Mvc.Rendering;

public class RolesController : Controller
{
    private readonly UserManager<CustomUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RolesController(UserManager<CustomUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }



    public IActionResult Index()
    {
        var roles = _roleManager.Roles.ToList();
        return View(roles);
    }






    // ایجاد نقش جدید
    public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
    {
        if (ModelState.IsValid)
        {
            var role = new IdentityRole(model.RoleName);
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                // Convert selected permissions to AccessLevel list
                var permissions = model.SelectedPermissions
                    .Select(p => (AccessLevel)Enum.Parse(typeof(AccessLevel), p))
                    .ToList();

                var roleEntity = new Role
                {
                    Name = model.RoleName,
                    Permissions = permissions
                };

                // _context.Roles.Add(roleEntity); // Uncomment when DbContext is available
                // await _context.SaveChangesAsync(); // Uncomment when DbContext is available

                return RedirectToAction("Index");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }

        return View(model);
    }



    // ویرایش نقش ها



   




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
}


