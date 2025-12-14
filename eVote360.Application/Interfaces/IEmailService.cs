using eVote360.Application.DTOs.Email;

namespace eVote360.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequestDto emailRequestDto);
    }
}