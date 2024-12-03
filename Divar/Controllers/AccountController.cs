namespace Divar.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DivarDbContext _context;

        public AccountController(UserManager<CustomUser> userManager,
                      SignInManager<CustomUser> signInManager,
                      RoleManager<IdentityRole> roleManager,
                      DivarDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }


        //درسترسی مجاز نیست
        public IActionResult AccessDenied()
        {
            return View();
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
                    return RedirectToAction("Login", "Account");
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
                                .Select(rp => rp.Permission.ToString()) 
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
    }
}
