using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlindMatchPAS.DTOs;
using BlindMatchPAS.DTOs.Api;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services;
using BlindMatchPAS.Services.Interfaces;

namespace BlindMatchPAS.Controllers.Api;

[Authorize(Roles = "Student")]
[Route("api/student")]
public class StudentApiController : ApiControllerBase
{
    private readonly IProposalService _proposalService;
    private readonly IResearchAreaService _researchAreaService;

    public StudentApiController(IProposalService proposalService, IResearchAreaService researchAreaService)
    {
        _proposalService = proposalService;
        _researchAreaService = researchAreaService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<StudentDashboardDto>> GetDashboard()
    {
        var proposals = (await _proposalService.GetProposalsByStudentAsync(GetCurrentUserId())).ToList();

        var dto = new StudentDashboardDto
        {
            TotalProposals = proposals.Count,
            PendingCount = proposals.Count(p => p.Status == ProposalStatus.Pending.ToString()),
            UnderReviewCount = proposals.Count(p => p.Status == ProposalStatus.UnderReview.ToString()),
            MatchedCount = proposals.Count(p => p.Status == ProposalStatus.Matched.ToString()),
            WithdrawnCount = proposals.Count(p => p.Status == ProposalStatus.Withdrawn.ToString()),
            RecentProposals = proposals
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToList()
        };

        return Ok(dto);
    }

    [HttpGet("proposals")]
    public async Task<ActionResult<IEnumerable<StudentProposalDto>>> GetProposals()
    {
        var proposals = await _proposalService.GetProposalsByStudentAsync(GetCurrentUserId());
        return Ok(proposals);
    }

    [HttpGet("proposals/{id:int}")]
    public async Task<ActionResult<StudentProposalDto>> GetProposalById(int id)
    {
        var proposal = (await _proposalService.GetProposalsByStudentAsync(GetCurrentUserId()))
            .FirstOrDefault(p => p.Id == id);

        if (proposal == null)
        {
            return NotFound(new ApiErrorDto { Message = "Proposal not found." });
        }

        return Ok(proposal);
    }

    [HttpPost("proposals")]
    public async Task<IActionResult> CreateProposal([FromBody] ProposalUpsertDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var proposal = new ProjectProposal
            {
                Title = request.Title,
                Abstract = request.Abstract,
                TechnicalStack = request.TechnicalStack,
                ResearchAreaId = request.ResearchAreaId
            };

            var created = await _proposalService.CreateProposalAsync(proposal, GetCurrentUserId());
            return CreatedAtAction(nameof(GetProposalById), new { id = created.Id }, new { id = created.Id });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ApiErrorDto { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorDto { Message = ex.Message });
        }
    }

    [HttpPut("proposals/{id:int}")]
    public async Task<IActionResult> UpdateProposal(int id, [FromBody] ProposalUpsertDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var proposal = new ProjectProposal
            {
                Id = id,
                Title = request.Title,
                Abstract = request.Abstract,
                TechnicalStack = request.TechnicalStack,
                ResearchAreaId = request.ResearchAreaId
            };

            var updated = await _proposalService.UpdateProposalAsync(proposal, GetCurrentUserId());
            if (updated == null)
            {
                return NotFound(new ApiErrorDto { Message = "Proposal not found." });
            }

            return Ok(new { message = "Proposal updated successfully." });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ApiErrorDto { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorDto { Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Message = ex.Message });
        }
    }

    [HttpPost("proposals/{id:int}/withdraw")]
    public async Task<IActionResult> WithdrawProposal(int id)
    {
        try
        {
            await _proposalService.WithdrawProposalAsync(id, GetCurrentUserId(), GetIpAddress());
            return Ok(new { message = "Proposal withdrawn successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorDto { Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDto { Message = ex.Message });
        }
    }

    [HttpGet("research-areas")]
    public async Task<IActionResult> GetResearchAreas()
    {
        var areas = await _researchAreaService.GetAllAsync();
        var dto = areas.Select(a => new ResearchAreaDto
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description
        });

        return Ok(dto);
    }
}
