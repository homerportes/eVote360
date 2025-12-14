namespace eVote360.Application.DTOs.ElectivePosition
{
    public class ElectivePositionDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required bool IsActive { get; set; } = true;
    }
}
