namespace eVote360.Application.ViewModels.PartyLeaderAssignment
{
    public class PartyLeaderAssignmentViewModel
    {
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public int PartyId { get; set; }
        public string PartyAcronym { get; set; } = string.Empty;
    }
}
