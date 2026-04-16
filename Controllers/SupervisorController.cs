using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services.Interfaces;

namespace BlindMatchPAS.Controllers;

[Authorize(Roles = "Supervisor")]
public class SupervisorController : Controller
{
    private readonly IProposalService _proposalService;
    private readonly IMatchingService _matchingService;
    private readonly IExpertiseService _expertiseService;
    private readonly IResearchAreaService _researchAreaService;

    public SupervisorController(
        IProposalService proposalService,
        IMatchingService matchingService,
        IExpertiseService expertiseService,
        IResearchAreaService researchAreaService)
    {
        _proposalService = proposalService;
        _matchingService = matchingService;
        _expertiseService = expertiseService;
        _researchAreaService = researchAreaService;
    }

    private string GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException();
    private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var interests = await _matchingService.GetSupervisorInterestsAsync(GetCurrentUserId());
        var confirmed = await _matchingService.GetSupervisorConfirmedMatchesAsync(GetCurrentUserId());
        var expertise = await _expertiseService.GetSupervisorExpertiseAsync(GetCurrentUserId());

        ViewBag.Interests = interests;
        ViewBag.ConfirmedMatches = confirmed;
        ViewBag.Expertise = expertise;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Browse()
    {
        var proposals = await _proposalService.GetAnonymousProposalsForSupervisorAsync(GetCurrentUserId());
        return View(proposals);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExpressInterest(int proposalId)
    {
        try
        {
            await _matchingService.ExpressInterestAsync(GetCurrentUserId(), proposalId, GetIpAddress());
            TempData["Success"] = "Interest expressed successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Browse));
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmMatch(int proposalId)
    {
        var proposal = await _matchingService.GetSupervisorInterestsAsync(GetCurrentUserId());
        var match = proposal.FirstOrDefault(m => m.ProposalId == proposalId);

        if (match == null)
        {
            return NotFound();
        }

        return View(match);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmMatch(int proposalId, bool confirmed)
    {
        if (!confirmed)
        {
            return RedirectToAction(nameof(Dashboard));
        }

        try
        {
            await _matchingService.ConfirmMatchAsync(GetCurrentUserId(), proposalId, GetIpAddress());
            TempData["Success"] = "Match confirmed! Student identity has been revealed.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public async Task<IActionResult> SetExpertise()
    {
        var allAreas = await _researchAreaService.GetAllAsync();
        var currentExpertise = await _expertiseService.GetSupervisorExpertiseAsync(GetCurrentUserId());

        ViewBag.AllAreas = allAreas;
        ViewBag.CurrentExpertise = currentExpertise.Select(e => e.Id).ToList();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetExpertise(List<int> areaIds)
    {
        try
        {
            await _expertiseService.SetSupervisorExpertiseAsync(GetCurrentUserId(), areaIds);
            TempData["Success"] = "Expertise areas updated successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Dashboard));
    }
}
