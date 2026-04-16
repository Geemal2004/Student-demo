using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Role != "Student" && model.Role != "Supervisor")
        {
            ModelState.AddModelError("", "Only Student or Supervisor roles can register.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, model.Role);
        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToAction("Dashboard");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        return RedirectToAction("Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Dashboard()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "Student";
            return RedirectToAction(GetDashboardAction(primaryRole), GetDashboardController(primaryRole));
        }

        return RedirectToAction("Index", "Home");
    }

    private string GetDashboardAction(string role)
    {
        return role switch
        {
            "Student" => "Dashboard",
            "Supervisor" => "Dashboard",
            "ModuleLeader" => "Dashboard",
            "SysAdmin" => "Dashboard",
            _ => "Index"
        };
    }

    private string GetDashboardController(string role)
    {
        return role switch
        {
            "Student" => "Student",
            "Supervisor" => "Supervisor",
            "ModuleLeader" => "ModuleLeader",
            "SysAdmin" => "Admin",
            _ => "Home"
        };
    }
}

public class RegisterViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Student";
}

public class LoginViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}
