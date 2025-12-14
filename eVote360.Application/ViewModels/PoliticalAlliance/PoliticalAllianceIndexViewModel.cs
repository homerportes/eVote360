using eVote360.Application.DTOs.PoliticalAlliance;

namespace eVote360.Application.ViewModels.PoliticalAlliance
{
    public class PoliticalAllianceIndexViewModel
    {
        public List<PoliticalAllianceDTO> SentRequests { get; set; } = new List<PoliticalAllianceDTO>();
        public List<PoliticalAllianceDTO> ReceivedRequests { get; set; } = new List<PoliticalAllianceDTO>();
        public int CurrentPartyId { get; set; }
    }
}
