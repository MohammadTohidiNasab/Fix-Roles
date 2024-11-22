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
    [HttpGet]
    public async Task<IActionResult> AssignRoleToUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "کاربر پیدا نشد.");
            return RedirectToAction("Index", "Roles"); // Redirect to the Roles index if the user is not found
        }

        var roles = _roleManager.Roles.ToList();
        var model = new AssignRoleViewModel
        {
            UserId = userId,
            AvailableRoles = roles.Select(r => r.Name).ToList() // Select role names
        };

        return View(model); // Pass the AssignRoleViewModel to the view
    }

    // اصلاح شده: اختصاص نقش به کاربر (POST)
    [HttpPost]
    public async Task<IActionResult> AssignRoleToUser(AssignRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var roles = _roleManager.Roles.ToList();
            model.AvailableRoles = roles.Select(r => r.Name).ToList(); // Populate available roles if model state is invalid
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "کاربر پیدا نشد.");
            return RedirectToAction("Index", "Roles");
        }

        // Add selected roles to the user
        foreach (var role in model.SelectedRoles)
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }

        return RedirectToAction("Index", "Roles");
    }


}
