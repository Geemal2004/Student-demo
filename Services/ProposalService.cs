using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.DTOs;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services.Interfaces;

namespace BlindMatchPAS.Services;

public class ProposalService : IProposalService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;

    public ProposalService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IAuditService auditService)
    {
        _context = context;
        _userManager = userManager;
        _auditService = auditService;
    }

    public async Task<ProjectProposal> CreateProposalAsync(ProjectProposal proposal, string studentId)
    {
        if (string.IsNullOrWhiteSpace(proposal.Title) || proposal.Title.Length < 10 || proposal.Title.Length > 150)
        {
            throw new ValidationException("Title must be between 10 and 150 characters.");
        }

        if (string.IsNullOrWhiteSpace(proposal.Abstract) || proposal.Abstract.Length < 100 || proposal.Abstract.Length > 1000)
        {
            throw new ValidationException("Abstract must be between 100 and 1000 characters.");
        }

        var user = await _userManager.FindByIdAsync(studentId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Student"))
        {
            throw new InvalidOperationException("Only students can create proposals.");
        }

        var area = await _context.ResearchAreas.FindAsync(proposal.ResearchAreaId);
        if (area == null)
        {
            throw new InvalidOperationException("Invalid research area.");
        }

        proposal.StudentId = studentId;
        proposal.Status = ProposalStatus.Pending;
        proposal.IsAnonymous = true;
        proposal.CreatedAt = DateTime.UtcNow;
        proposal.UpdatedAt = DateTime.UtcNow;

        _context.Proposals.Add(proposal);
        await _context.SaveChangesAsync();

        return proposal;
    }

    public async Task<IEnumerable<StudentProposalDto>> GetProposalsByStudentAsync(string studentId)
    {
        var proposals = await _context.Proposals
            .Include(p => p.ResearchArea)
            .Include(p => p.SupervisorMatches)
            .Where(p => p.StudentId == studentId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var result = new List<StudentProposalDto>();
        foreach (var p in proposals)
        {
            var dto = new StudentProposalDto
            {
                Id = p.Id,
                Title = p.Title,
                Abstract = p.Abstract,
                TechnicalStack = p.TechnicalStack,
                ResearchAreaName = p.ResearchArea?.Name ?? "",
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt
            };

            // If matched, get revealed supervisor info
            if (p.Status == ProposalStatus.Matched)
            {
                var match = await _context.SupervisorMatches
                    .Include(m => m.Supervisor)
                    .FirstOrDefaultAsync(m => m.ProposalId == p.Id && m.IsRevealed);

                if (match != null)
                {
                    dto.MatchedSupervisor = new RevealedMatchDto
                    {
                        MatchId = match.Id,
                        SupervisorName = match.Supervisor?.FullName ?? "",
                        SupervisorEmail = match.Supervisor?.Email ?? "",
                        ConfirmedAt = match.ConfirmedAt
                    };
                }
            }

            result.Add(dto);
        }

        return result;
    }

    public async Task<ProjectProposal?> UpdateProposalAsync(ProjectProposal proposal, string studentId)
    {
        var existing = await _context.Proposals.FindAsync(proposal.Id);
        if (existing == null)
        {
            throw new InvalidOperationException("Proposal not found.");
        }

        if (existing.StudentId != studentId)
        {
            throw new UnauthorizedAccessException("You can only edit your own proposals.");
        }

        if (existing.Status != ProposalStatus.Pending)
        {
            throw new InvalidOperationException("Only pending proposals can be edited.");
        }

        if (string.IsNullOrWhiteSpace(proposal.Title) || proposal.Title.Length < 10 || proposal.Title.Length > 150)
        {
            throw new ValidationException("Title must be between 10 and 150 characters.");
        }

        if (string.IsNullOrWhiteSpace(proposal.Abstract) || proposal.Abstract.Length < 100 || proposal.Abstract.Length > 1000)
        {
            throw new ValidationException("Abstract must be between 100 and 1000 characters.");
        }

        existing.Title = proposal.Title;
        existing.Abstract = proposal.Abstract;
        existing.TechnicalStack = proposal.TechnicalStack;
        existing.ResearchAreaId = proposal.ResearchAreaId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task WithdrawProposalAsync(int proposalId, string studentId, string? ipAddress = null)
    {
        var proposal = await _context.Proposals.FindAsync(proposalId);
        if (proposal == null)
        {
            throw new InvalidOperationException("Proposal not found.");
        }

        if (proposal.StudentId != studentId)
        {
            throw new UnauthorizedAccessException("You can only withdraw your own proposals.");
        }

        if (proposal.Status == ProposalStatus.Matched)
        {
            throw new InvalidOperationException("Cannot withdraw a matched proposal.");
        }

        var oldStatus = proposal.Status;
        proposal.Status = ProposalStatus.Withdrawn;
        proposal.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            "ProposalWithdrawn",
            nameof(ProjectProposal),
            proposalId.ToString(),
            studentId,
            new { OldStatus = oldStatus.ToString() },
            new { NewStatus = ProposalStatus.Withdrawn.ToString() },
            ipAddress);
    }

    public async Task<IEnumerable<BlindProposalDto>> GetAnonymousProposalsForSupervisorAsync(string supervisorId)
    {
        // Get supervisor's expertise areas - NO loading of Student entity
        var supervisorAreas = await _context.SupervisorExpertises
            .Where(se => se.SupervisorId == supervisorId)
            .Select(se => se.ResearchAreaId)
            .ToListAsync();

        // Project directly to BlindProposalDto - Student identity never loaded
        var proposals = await _context.Proposals
            .Where(p => (p.Status == ProposalStatus.Pending || p.Status == ProposalStatus.UnderReview)
                        && supervisorAreas.Contains(p.ResearchAreaId))
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new BlindProposalDto
            {
                Id = p.Id,
                Title = p.Title,
                Abstract = p.Abstract,
                TechnicalStack = p.TechnicalStack,
                ResearchAreaName = p.ResearchArea != null ? p.ResearchArea.Name : "Unknown"
                // ⛔ SECURITY: StudentId, StudentName, StudentEmail - NEVER projected
            })
            .ToListAsync();

        return proposals;
    }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
