using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.DTOs;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services.Interfaces;

namespace BlindMatchPAS.Controllers;

[Authorize(Roles = "SysAdmin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserManagementService _userManagementService;

    public AdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IUserManagementService userManagementService)
    {
        _context = context;
        _userManager = userManager;
        _userManagementService = userManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var dashboard = new AdminDashboardDto();

        dashboard.TotalUsers = await _context.Users.CountAsync();
        dashboard.TotalProposals = await _context.Proposals.CountAsync();
        dashboard.PendingProposals = await _context.Proposals.CountAsync(p => p.Status == ProposalStatus.Pending);
        dashboard.TotalMatches = await _context.SupervisorMatches.CountAsync();
        dashboard.ConfirmedMatches = await _context.SupervisorMatches.CountAsync(m => m.IsConfirmed);

        return View(dashboard);
    }

    [HttpGet]
    public IActionResult UserManagement()
    {
        return View();
    }

    [HttpGet]
    public IActionResult CreateUser()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var user = await _userManagementService.CreateAccountAsync(
                model.Role, model.Email, model.Password, model.FullName);

            TempData["Success"] = $"User {user.Email} created successfully.";
            return RedirectToAction(nameof(UserManagement));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUsersByRole(string role)
    {
        var users = await _userManagementService.GetUsersByRoleAsync(role);
        var userList = users.Select(u => new UserListDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email ?? "",
            Role = role,
            IsActive = u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow
        }).ToList();

        return PartialView("_UserListPartial", userList);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateUser(string userId)
    {
        var result = await _userManagementService.DeactivateAccountAsync(userId);
        if (result)
        {
            TempData["Success"] = "User deactivated successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to deactivate user.";
        }

        return RedirectToAction(nameof(UserManagement));
    }

    [HttpGet]
    public IActionResult MigrationStatus()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetMigrationStatus()
    {
        var migrations = await _context.Database
            .SqlQueryRaw<string>("SELECT MigrationId FROM __EFMigrationsHistory")
            .ToListAsync();

        return Json(new { migrations });
    }
}

public class CreateUserViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Student";
}
