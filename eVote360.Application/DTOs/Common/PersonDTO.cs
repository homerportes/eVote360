namespace eVote360.Application.DTOs.Common
{
    public class PersonDTO<TKey>
    {
        public required TKey Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; } = null!;
        public required bool IsActive { get; set; } = true;
    }
}
