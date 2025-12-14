using eVote360.Domain.Common.Enums;

namespace eVote360.Domain.Entities
{
    public class PoliticalAlliance
    {
        public int Id { get; set; }
        public int RequestingPartyId { get; set; }
        public int ReceivingPartyId { get; set; }
        public AllianceStatus Status { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }

        public PoliticalParty RequestingParty { get; set; } = null!;
        public PoliticalParty ReceivingParty { get; set; } = null!;
    }
}
