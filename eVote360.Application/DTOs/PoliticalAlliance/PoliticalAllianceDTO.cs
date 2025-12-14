using eVote360.Domain.Common.Enums;

namespace eVote360.Application.DTOs.PoliticalAlliance
{
    public class PoliticalAllianceDTO
    {
        public int Id { get; set; }
        public int RequestingPartyId { get; set; }
        public string RequestingPartyName { get; set; } = string.Empty;
        public string RequestingPartyAcronym { get; set; } = string.Empty;
        public int ReceivingPartyId { get; set; }
        public string ReceivingPartyName { get; set; } = string.Empty;
        public string ReceivingPartyAcronym { get; set; } = string.Empty;
        public AllianceStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }
}
