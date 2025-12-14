namespace eVote360.Application.DTOs.Candidacy
{
    public class SaveCandidacyDTO
    {
        public int Id { get; set; }
        public required int CandidateId { get; set; }
        public required int ElectivePositionId { get; set; }
        public required int PostulatingPartyId { get; set; }
        public required bool IsAlliance { get; set; }
    }
}
