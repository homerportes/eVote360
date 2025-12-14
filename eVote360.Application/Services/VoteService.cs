using eVote360.Application.DTOs.Vote;
using eVote360.Application.Interfaces;
using eVote360.Domain.Entities;
using eVote360.Domain.Interfaces;

namespace eVote360.Application.Services
{
    /// <summary>
    /// Vote Service implementation
    /// Single Responsibility Principle: Only manages vote operations
    /// Dependency Inversion Principle: Depends on abstractions (interfaces)
    /// </summary>
    public class VoteService : IVoteService
    {
        private readonly IVoteRepository _voteRepository;
        private readonly IElectionRepository _electionRepository;
        private readonly ICitizenRepository _citizenRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IElectivePositionRepository _positionRepository;

        public VoteService(
            IVoteRepository voteRepository,
            IElectionRepository electionRepository,
            ICitizenRepository citizenRepository,
            ICandidateRepository candidateRepository,
            IElectivePositionRepository positionRepository)
        {
            _voteRepository = voteRepository;
            _electionRepository = electionRepository;
            _citizenRepository = citizenRepository;
            _candidateRepository = candidateRepository;
            _positionRepository = positionRepository;
        }

        public async Task<bool> HasCitizenVotedAsync(int citizenId, int electionId)
        {
            var votes = await _voteRepository.GetAllList();
            return votes.Any(v => v.CitizenId == citizenId && v.ElectionId == electionId);
        }

        public async Task<bool> RegisterVoteAsync(SaveVoteDTO voteDto)
        {
            try
            {
                var vote = new Vote
                {
                    ElectionId = voteDto.ElectionId,
                    CitizenId = voteDto.CitizenId,
                    CandidateId = voteDto.CandidateId,
                    ElectivePositionId = voteDto.ElectivePositionId,
                    VoteDate = DateTime.UtcNow
                };

                var created = await _voteRepository.AddAsync(vote);
                return created != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<VoteDTO>> GetVotesByCitizenAsync(int citizenId, int electionId)
        {
            var votes = await _voteRepository.GetAllList();
            var citizenVotes = votes.Where(v => v.CitizenId == citizenId && v.ElectionId == electionId).ToList();

            var voteDtos = new List<VoteDTO>();

            foreach (var vote in citizenVotes)
            {
                var dto = await MapToDTO(vote);
                if (dto != null)
                {
                    voteDtos.Add(dto);
                }
            }

            return voteDtos;
        }

        public async Task<int> CountVotesByElectionAsync(int electionId)
        {
            var votes = await _voteRepository.GetAllList();
            // Count unique citizens who voted
            return votes.Where(v => v.ElectionId == electionId)
                       .Select(v => v.CitizenId)
                       .Distinct()
                       .Count();
        }

        private async Task<VoteDTO?> MapToDTO(Vote vote)
        {
            var election = await _electionRepository.GetById(vote.ElectionId);
            var citizen = await _citizenRepository.GetById(vote.CitizenId);
            var candidate = vote.CandidateId.HasValue ? await _candidateRepository.GetById(vote.CandidateId.Value) : null;
            var position = await _positionRepository.GetById(vote.ElectivePositionId);

            if (election == null || citizen == null || candidate == null || position == null)
            {
                return null;
            }

            return new VoteDTO
            {
                Id = vote.Id,
                ElectionId = vote.ElectionId,
                ElectionName = election.Name,
                CitizenId = vote.CitizenId,
                CitizenName = $"{citizen.FirstName} {citizen.LastName}",
                CandidateId = vote.CandidateId ?? 0,
                CandidateName = candidate != null ? $"{candidate.Citizen?.FirstName} {candidate.Citizen?.LastName}" : "",
                ElectivePositionId = vote.ElectivePositionId,
                PositionName = position.Name,
                PartyAcronym = candidate?.Party?.Acronym ?? "",
                VotedAt = vote.VoteDate
            };
        }
    }
}
