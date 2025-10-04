using System.Linq;
using System.Threading.Tasks;
using EduResolve.Data;
using EduResolve.Models;
using EduResolve.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduResolve.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var viewModel = new RegisterViewModel
            {
                RoleOptions = new[]
                {
                    new SelectListItem { Text = "Student", Value = "Student" },
                    new SelectListItem { Text = "Teacher", Value = "Teacher" }
                },
                DepartmentOptions = await _context.Departments
                    .OrderBy(d => d.Name)
                    .Select(d => new SelectListItem(d.Name, d.Id.ToString()))
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel viewModel)
        {
            if (!string.IsNullOrWhiteSpace(viewModel.Role)
                && (viewModel.Role.Equals("Student") || viewModel.Role.Equals("Teacher"))
                && !viewModel.DepartmentId.HasValue)
            {
                ModelState.AddModelError(nameof(viewModel.DepartmentId), "Please select a department for the chosen role.");
            }

            if (!ModelState.IsValid)
            {
                viewModel.RoleOptions = new[]
                {
                    new SelectListItem { Text = "Student", Value = "Student" },
                    new SelectListItem { Text = "Teacher", Value = "Teacher" }
                };
                viewModel.DepartmentOptions = await _context.Departments
                    .OrderBy(d => d.Name)
                    .Select(d => new SelectListItem(d.Name, d.Id.ToString()))
                    .ToListAsync();

                return View(viewModel);
            }

            var user = new ApplicationUser
            {
                UserName = viewModel.Email,
                Email = viewModel.Email,
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                DepartmentId = viewModel.DepartmentId
            };

            var result = await _userManager.CreateAsync(user, viewModel.Password);

            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(viewModel.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(viewModel.Role));
                }

                await _userManager.AddToRoleAsync(user, viewModel.Role);

                _logger.LogInformation("User created a new account with role {Role}.", viewModel.Role);

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            viewModel.RoleOptions = new[]
            {
                new SelectListItem { Text = "Student", Value = "Student" },
                new SelectListItem { Text = "Teacher", Value = "Teacher" }
            };
            viewModel.DepartmentOptions = await _context.Departments
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem(d.Name, d.Id.ToString()))
                .ToListAsync();

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            var viewModel = new LoginViewModel { ReturnUrl = returnUrl };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var result = await _signInManager.PasswordSignInAsync(viewModel.Email, viewModel.Password, viewModel.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                if (!string.IsNullOrEmpty(viewModel.ReturnUrl) && Url.IsLocalUrl(viewModel.ReturnUrl))
                {
                    return Redirect(viewModel.ReturnUrl);
                }

                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
