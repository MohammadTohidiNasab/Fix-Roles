using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Divar.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DivarDbContext _context;

        public UserController(UserManager<CustomUser> userManager,
                              SignInManager<CustomUser> signInManager,
                              RoleManager<IdentityRole> roleManager,
                              DivarDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        // Register controller
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new CustomUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("Login", "User");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // Login controller
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Email, user.Email),
                    };

                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));

                        // Fetch the role to get its permissions
                        var role = await _roleManager.FindByNameAsync(userRole);
                        if (role != null)
                        {
                            var rolePermissions = _context.RolePermissions
                                .Where(rp => rp.RoleId == role.Id)
                                .Select(rp => rp.Permission.ToString()) // Get the permission as a string
                                .ToList();

                            // Add each permission as a separate claim
                            foreach (var permission in rolePermissions)
                            {
                                authClaims.Add(new Claim("Permission", permission));
                            }
                        }
                    }

                    await _signInManager.SignInWithClaimsAsync(user, model.RememberMe, authClaims);

                    // Set session variables
                    HttpContext.Session.SetString("FirstName", user.FirstName);
                    HttpContext.Session.SetString("LastName", user.LastName);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserId", user.Id);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        // Logout
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("FirstName");
            HttpContext.Session.Remove("LastName");
            HttpContext.Session.Remove("UserEmail");
            HttpContext.Session.Remove("UserId");

            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        // Profile
        [HttpGet]
        public IActionResult Profile()
        {
            var userClaims = User.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            }).ToList();

            // دریافت سشن‌های مرتبط با کاربر
            var sessionData = HttpContext.Session.Keys
                .Select(key => new
                {
                    Key = key,
                    Value = HttpContext.Session.GetString(key) // assuming the session values are stored as strings
                }).ToList();

            // ترکیب اطلاعات کاربر با سشن‌ها
            var model = new
            {
                UserClaims = userClaims,
                SessionData = sessionData
            };

            return View(model);
        }
    }
}
