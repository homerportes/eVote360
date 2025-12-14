namespace eVote360.Application.DTOs.Common
{
    public class DashboardIndicatorsDTO
    {
        public int ActiveCandidates { get; set; }
        public int InactiveCandidates { get; set; }
        public int TotalAlliances { get; set; }
        public int PendingAllianceRequests { get; set; }
        public int CandidatesAssignedToPositions { get; set; }
    }
}
