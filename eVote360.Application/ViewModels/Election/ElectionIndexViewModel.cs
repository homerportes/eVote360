using eVote360.Application.DTOs.Election;

namespace eVote360.Application.ViewModels.Election
{
    public class ElectionIndexViewModel
    {
        public List<ElectionDTO> Elections { get; set; } = new List<ElectionDTO>();
        public bool HasActiveElection { get; set; }
    }
}
