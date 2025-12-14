namespace eVote360.Application.DTOs.Election
{
    public class CandidateResultDTO
    {
        public int CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string PartyAcronym { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public int VotesReceived { get; set; }
        public decimal VotePercentage { get; set; }
    }
}
