using eVote360.Domain.Common.Enums;

namespace eVote360.Application.DTOs.Election
{
    public class ElectionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public ElectionStatus Status { get; set; }
        public int ParticipatingPartiesCount { get; set; }
        public int DisputedPositionsCount { get; set; }
    }
}
