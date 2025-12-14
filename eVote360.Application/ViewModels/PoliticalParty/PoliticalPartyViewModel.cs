namespace eVote360.Application.ViewModels.PoliticalParty
{
    public class PoliticalPartyViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Acronym { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
