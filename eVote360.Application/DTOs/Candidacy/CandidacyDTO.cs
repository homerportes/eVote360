namespace eVote360.Application.DTOs.Candidacy
{
    public class CandidacyDTO
    {
        public int Id { get; set; }
        public int CandidateId { get; set; }
        public string CandidateFirstName { get; set; } = string.Empty;
        public string CandidateLastName { get; set; } = string.Empty;
        public int ElectivePositionId { get; set; }
        public string ElectivePositionName { get; set; } = string.Empty;
        public int PostulatingPartyId { get; set; }
        public string PostulatingPartyName { get; set; } = string.Empty;
        public string PostulatingPartyAcronym { get; set; } = string.Empty;
        public bool IsAlliance { get; set; }
    }
}
