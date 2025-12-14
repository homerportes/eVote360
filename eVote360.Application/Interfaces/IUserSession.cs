using eVote360.Application.ViewModels.User;

namespace eVote360.Application.Interfaces
{
    public interface IUserSession
    {
        UserViewModel? GetUserSession();
        bool HasUser();
        bool IsAdmin();
        bool IsDirigente();
    }
}
