using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlindMatchPAS.DTOs;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services.Interfaces;

namespace BlindMatchPAS.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly IProposalService _proposalService;
    private readonly IMatchingService _matchingService;
    private readonly IResearchAreaService _researchAreaService;

    public StudentController(
        IProposalService proposalService,
        IMatchingService matchingService,
        IResearchAreaService researchAreaService)
    {
        _proposalService = proposalService;
        _matchingService = matchingService;
        _researchAreaService = researchAreaService;
    }

    private string GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException();
    private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var proposals = await _proposalService.GetProposalsByStudentAsync(GetCurrentUserId());
        return View(proposals);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var areas = await _researchAreaService.GetAllAsync();
        ViewBag.ResearchAreas = areas;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] ProjectProposal model)
    {
        if (!ModelState.IsValid)
        {
            var areas = await _researchAreaService.GetAllAsync();
            ViewBag.ResearchAreas = areas;
            return View(model);
        }

        try
        {
            await _proposalService.CreateProposalAsync(model, GetCurrentUserId());
            TempData["Success"] = "Proposal created successfully.";
            return RedirectToAction(nameof(Dashboard));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            var areas = await _researchAreaService.GetAllAsync();
            ViewBag.ResearchAreas = areas;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var proposals = await _proposalService.GetProposalsByStudentAsync(GetCurrentUserId());
        var proposal = proposals.FirstOrDefault(p => p.Id == id);

        if (proposal == null)
        {
            return NotFound();
        }

        if (proposal.Status != nameof(ProposalStatus.Pending))
        {
            TempData["Error"] = "Only pending proposals can be edited.";
            return RedirectToAction(nameof(Dashboard));
        }

        var areas = await _researchAreaService.GetAllAsync();
        ViewBag.ResearchAreas = areas;

        // Convert DTO back to entity for edit form
        var editModel = new ProjectProposal
        {
            Id = proposal.Id,
            Title = proposal.Title,
            Abstract = proposal.Abstract,
            TechnicalStack = proposal.TechnicalStack,
            ResearchAreaId = (await _researchAreaService.GetAllAsync()).First(a => a.Name == proposal.ResearchAreaName).Id
        };

        return View(editModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromForm] ProjectProposal model)
    {
        if (!ModelState.IsValid)
        {
            var areas = await _researchAreaService.GetAllAsync();
            ViewBag.ResearchAreas = areas;
            return View(model);
        }

        try
        {
            await _proposalService.UpdateProposalAsync(model, GetCurrentUserId());
            TempData["Success"] = "Proposal updated successfully.";
            return RedirectToAction(nameof(Dashboard));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            var areas = await _researchAreaService.GetAllAsync();
            ViewBag.ResearchAreas = areas;
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int id)
    {
        try
        {
            await _proposalService.WithdrawProposalAsync(id, GetCurrentUserId(), GetIpAddress());
            TempData["Success"] = "Proposal withdrawn successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var proposals = await _proposalService.GetProposalsByStudentAsync(GetCurrentUserId());
        var proposal = proposals.FirstOrDefault(p => p.Id == id);

        if (proposal == null)
        {
            return NotFound();
        }

        return View(proposal);
    }
}
