using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BlindMatchPAS.Data;
using BlindMatchPAS.DTOs;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Controllers;

[Authorize(Roles = "ModuleLeader")]
public class ModuleLeaderController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IResearchAreaService _researchAreaService;
    private readonly IUserManagementService _userManagementService;
    private readonly IMatchingService _matchingService;

    public ModuleLeaderController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IResearchAreaService researchAreaService,
        IUserManagementService userManagementService,
        IMatchingService matchingService)
    {
        _context = context;
        _userManager = userManager;
        _researchAreaService = researchAreaService;
        _userManagementService = userManagementService;
        _matchingService = matchingService;
    }

    private string GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException();
    private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpGet]
    public async Task<IActionResult> Dashboard(string? statusFilter = null)
    {
        var dashboard = new ModuleLeaderDashboardDto();

        var students = await _userManagementService.GetUsersByRoleAsync("Student");
        var supervisors = await _userManagementService.GetUsersByRoleAsync("Supervisor");
        dashboard.TotalStudents = students.Count();
        dashboard.TotalSupervisors = supervisors.Count();
        dashboard.TotalProposals = await _context.Proposals.CountAsync();

        var confirmedMatches = await _context.SupervisorMatches
            .Include(m => m.Proposal)
            .ThenInclude(p => p!.Student)
            .Include(m => m.Supervisor)
            .Include(m => m.Proposal)
            .ThenInclude(p => p!.ResearchArea)
            .Where(m => m.IsConfirmed)
            .OrderByDescending(m => m.ConfirmedAt)
            .ToListAsync();

        dashboard.TotalMatches = confirmedMatches.Count;
        dashboard.Matches = confirmedMatches.Select(m => new MatchOverviewDto
        {
            MatchId = m.Id,
            StudentName = m.Proposal?.Student?.FullName ?? "",
            StudentEmail = m.Proposal?.Student?.Email ?? "",
            SupervisorName = m.Supervisor?.FullName ?? "",
            SupervisorEmail = m.Supervisor?.Email ?? "",
            ProjectTitle = m.Proposal?.Title ?? "",
            ResearchArea = m.Proposal?.ResearchArea?.Name ?? "",
            Status = m.Proposal?.Status.ToString() ?? "",
            IsConfirmed = m.IsConfirmed,
            ConfirmedAt = m.ConfirmedAt
        }).ToList();

        if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<ProposalStatus>(statusFilter, out var status))
        {
            dashboard.Matches = dashboard.Matches.Where(m => m.Status == status.ToString()).ToList();
        }

        ViewBag.StatusFilter = statusFilter;
        return View(dashboard);
    }

    [HttpGet]
    public async Task<IActionResult> ResearchAreas()
    {
        var areas = await _researchAreaService.GetAllAsync();
        return View(areas);
    }

    [HttpGet]
    public IActionResult CreateResearchArea()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateResearchArea(ResearchArea model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _researchAreaService.CreateAsync(model);
            TempData["Success"] = "Research area created successfully.";
            return RedirectToAction(nameof(ResearchAreas));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditResearchArea(int id)
    {
        var area = await _researchAreaService.GetByIdAsync(id);
        if (area == null)
        {
            return NotFound();
        }
        return View(area);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditResearchArea(ResearchArea model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _researchAreaService.UpdateAsync(model);
            TempData["Success"] = "Research area updated successfully.";
            return RedirectToAction(nameof(ResearchAreas));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteResearchArea(int id)
    {
        await _researchAreaService.DeleteAsync(id);
        TempData["Success"] = "Research area deactivated.";
        return RedirectToAction(nameof(ResearchAreas));
    }

    [HttpGet]
    public async Task<IActionResult> Users(string? roleFilter = null)
    {
        IEnumerable<ApplicationUser> users;

        if (!string.IsNullOrEmpty(roleFilter))
        {
            users = await _userManagementService.GetUsersByRoleAsync(roleFilter);
        }
        else
        {
            var students = await _userManagementService.GetUsersByRoleAsync("Student");
            var supervisors = await _userManagementService.GetUsersByRoleAsync("Supervisor");
            users = students.Concat(supervisors);
        }

        // Project to DTOs with role information
        var userDtos = new List<UserListDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserListDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                Role = roles.FirstOrDefault() ?? "Unknown",
                IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow
            });
        }

        ViewBag.RoleFilter = roleFilter;
        return View(userDtos);
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableSupervisors()
    {
        var supervisors = await _userManagementService.GetUsersByRoleAsync("Supervisor");
        return Json(supervisors.Select(s => new { s.Id, s.FullName, s.Email }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reassign(int matchId, string newSupervisorId)
    {
        try
        {
            await _matchingService.ReassignMatchAsync(matchId, newSupervisorId, GetCurrentUserId(), GetIpAddress());
            TempData["Success"] = "Project reassigned successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Dashboard));
    }
}
