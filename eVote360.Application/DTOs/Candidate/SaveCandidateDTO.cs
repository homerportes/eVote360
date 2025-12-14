namespace eVote360.Application.DTOs.Candidate
{
    public class SaveCandidateDTO
    {
        public int Id { get; set; }
        public int CitizenId { get; set; }
        public int PartyId { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
