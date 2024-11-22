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
    public async Task<IActionResult> CreateRole(string roleName, List<AccessLevel> permissions)
    {
        var role = new IdentityRole(roleName);
        await _roleManager.CreateAsync(role);

        // تخصیص دسترسی‌ها به نقش
        var roleEntity = new Role
        {
            Name = roleName,
            Permissions = permissions
        };

        // ذخیره کردن نقش با دسترسی‌ها در دیتابیس (مثال برای یک جدول موجود)
        // _context.Roles.Add(roleEntity);
        // await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // اختصاص نقش به کاربر
    public async Task<IActionResult> AssignRoleToUser(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        await _userManager.AddToRoleAsync(user, roleName);
        return RedirectToAction("Index");
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

}
