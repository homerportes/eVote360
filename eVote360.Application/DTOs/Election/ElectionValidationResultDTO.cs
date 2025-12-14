namespace eVote360.Application.DTOs.Election
{
    public class ElectionValidationResultDTO
    {
        public bool IsValid { get; set; }
        public List<string> ValidationMessages { get; set; } = new List<string>();
        public bool HasSufficientParties { get; set; }
        public bool AllPartiesHaveCandidatesForAllPositions { get; set; }
    }
}
