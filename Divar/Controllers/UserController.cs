
using System.Security.Claims;

namespace Divar.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<CustomUser> _userManager;
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(IUserRepository userRepository,
                              UserManager<CustomUser> userManager,
                              SignInManager<CustomUser> signInManager,
                              RoleManager<IdentityRole> roleManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // Register (unchanged)
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
                var emailExists = await _userRepository.EmailExistsAsync(model.Email);
                if (emailExists)
                {
                    ModelState.AddModelError("", "ایمیل قبلا ثبت شده است");
                    return View(model);
                }

                var user = new CustomUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password) // رمزگذاری
                };

                await _userRepository.AddUserAsync(user);
                return RedirectToAction("Login", "User");
            }
            return View(model);
        }

        // Login (updated)
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
                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
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
                        var role = await _roleManager.FindByNameAsync(userRole);
                        if (role != null)
                        {
                            var roleClaims = await _roleManager.GetClaimsAsync(role);
                            foreach (var roleClaim in roleClaims)
                            {
                                if (roleClaim.Type == "Permission")
                                {
                                    authClaims.Add(new Claim("Permission", roleClaim.Value));
                                }
                            }
                        }
                    }

                    await _signInManager.SignInWithClaimsAsync(user, model.RememberMe, authClaims);

                    HttpContext.Session.SetString("FirstName", user.FirstName ?? "");
                    HttpContext.Session.SetString("LastName", user.LastName ?? "");
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserId", user.Id);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "ورود نامعتبر");
            }
            return View(model);
        }

        // Logout (unchanged)
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}