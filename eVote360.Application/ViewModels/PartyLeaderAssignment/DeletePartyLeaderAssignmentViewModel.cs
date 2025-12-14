namespace eVote360.Application.ViewModels.PartyLeaderAssignment
{
    public class DeletePartyLeaderAssignmentViewModel
    {
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public string PartyAcronym { get; set; } = string.Empty;
    }
}
