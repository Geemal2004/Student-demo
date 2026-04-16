using BlindMatchPAS.DTOs;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Services.Interfaces;

public interface IProposalService
{
    Task<ProjectProposal> CreateProposalAsync(ProjectProposal proposal, string studentId);
    Task<IEnumerable<StudentProposalDto>> GetProposalsByStudentAsync(string studentId);
    Task<ProjectProposal?> UpdateProposalAsync(ProjectProposal proposal, string studentId);
    Task WithdrawProposalAsync(int proposalId, string studentId, string? ipAddress = null);
    Task<IEnumerable<BlindProposalDto>> GetAnonymousProposalsForSupervisorAsync(string supervisorId);
}
