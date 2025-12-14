using eVote360.Application.DTOs.Election;

namespace eVote360.Application.ViewModels.HomeAdmin
{
    public class HomeAdminIndexViewModel
    {
        public List<int> AvailableYears { get; set; } = new();
        public int? SelectedYear { get; set; }
        public List<ElectionSummaryDTO> ElectionSummaries { get; set; } = new();
    }
}
