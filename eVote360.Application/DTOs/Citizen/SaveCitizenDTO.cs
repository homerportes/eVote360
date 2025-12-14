using eVote360.Application.DTOs.Common;

namespace eVote360.Application.DTOs.Citizen
{
    public class SaveCitizenDTO : PersonDTO<int>
    {
        public required string IdentityDocument { get; set; }
    }
}
