
namespace eVote360.Domain.Common.CommonEntities
{
    public class Person<TKey>
    {
        public required TKey Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; } = null!;
        public required bool IsActive { get; set; } = true;
    }
}
