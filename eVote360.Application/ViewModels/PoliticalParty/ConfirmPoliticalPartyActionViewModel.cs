namespace eVote360.Application.ViewModels.PoliticalParty
{
    public class ConfirmPoliticalPartyActionViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
