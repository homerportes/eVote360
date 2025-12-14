namespace eVote360.Application.Interfaces
{
    /// <summary>
    /// Interface for voting validations
    /// Single Responsibility: Validates voting business rules
    /// </summary>
    public interface IVotingValidator
    {
        Task<VotingValidationResult> ValidateCitizenCanVoteAsync(string documentNumber);
        Task<VotingValidationResult> ValidateCompleteVotingAsync(int citizenId, int electionId, Dictionary<int, int> votesPerPosition);
    }

    public class VotingValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? CitizenId { get; set; }
        public int? ElectionId { get; set; }
        public List<string> MissingPositions { get; set; } = new();
    }
}
