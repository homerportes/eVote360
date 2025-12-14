namespace eVote360.Application.ViewModels.ElectivePosition
{
    public class ElectivePositionViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
