namespace eVote360.Application.ViewModels.Common
{
    public class ConfirmPersonActionViewModel<TKey>
    {
        public required TKey Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public bool IsActive { get; set; }
    }
}
