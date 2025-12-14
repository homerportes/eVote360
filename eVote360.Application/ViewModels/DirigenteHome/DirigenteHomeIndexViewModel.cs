using eVote360.Application.DTOs.Common;
using eVote360.Application.DTOs.PoliticalParty;

namespace eVote360.Application.ViewModels.DirigenteHome
{
    public class DirigenteHomeIndexViewModel
    {
        public PoliticalPartyDTO? Party { get; set; }
        public DashboardIndicatorsDTO Indicators { get; set; } = new();
    }
}
