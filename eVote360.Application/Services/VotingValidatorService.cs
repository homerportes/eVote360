using eVote360.Application.Interfaces;
using eVote360.Domain.Common.Enums;

namespace eVote360.Application.Services
{
    /// <summary>
    /// Voting Validator Service
    /// Single Responsibility Principle: Only validates voting business rules
    /// Open/Closed Principle: Open for extension, closed for modification
    /// </summary>
    public class VotingValidatorService : IVotingValidator
    {
        private readonly ICitizenService _citizenService;
        private readonly IElectionService _electionService;
        private readonly IVoteService _voteService;
        private readonly IElectivePositionService _positionService;

        public VotingValidatorService(
            ICitizenService citizenService,
            IElectionService electionService,
            IVoteService voteService,
            IElectivePositionService positionService)
        {
            _citizenService = citizenService;
            _electionService = electionService;
            _voteService = voteService;
            _positionService = positionService;
        }

        public async Task<VotingValidationResult> ValidateCitizenCanVoteAsync(string documentNumber)
        {
            var result = new VotingValidationResult { IsValid = false };

            // 1. Get active election
            var activeElection = await _electionService.GetActiveElectionAsync();
            if (activeElection == null)
            {
                result.Message = "No hay ningÃºn proceso electoral en estos momentos.";
                return result;
            }

            // 2. Find citizen by document number
            var citizens = await _citizenService.GetAll();
            var citizen = citizens.FirstOrDefault(c => c.IdentityDocument == documentNumber);

            if (citizen == null)
            {
                result.Message = "Ciudadano no encontrado en el sistema.";
                return result;
            }

            // 3. Check if citizen is active
            if (!citizen.IsActive)
            {
                result.Message = "Este ciudadano estÃ¡ inactivo y no puede participar en el proceso electoral.";
                return result;
            }

            // 4. Check if citizen has already voted
            var hasVoted = await _voteService.HasCitizenVotedAsync(citizen.Id, activeElection.Id);
            if (hasVoted)
            {
                result.Message = "Ya ha ejercido su derecho al voto.";
                return result;
            }

            // All validations passed
            result.IsValid = true;
            result.CitizenId = citizen.Id;
            result.ElectionId = activeElection.Id;
            result.Message = "ValidaciÃ³n exitosa. Puede proceder a votar.";

            return result;
        }

        public async Task<VotingValidationResult> ValidateCompleteVotingAsync(
            int citizenId,
            int electionId,
            Dictionary<int, int> votesPerPosition)
        {
            var result = new VotingValidationResult { IsValid = false };

            // Get all active elective positions
            var allPositions = await _positionService.GetAll();
            var activePositions = allPositions.Where(p => p.IsActive).ToList();

            // Get positions that are part of this election
            // (positions with candidates assigned)
            var electionCandidacies = await GetElectionPositionsAsync(electionId);
            var electionPositionIds = electionCandidacies.Select(ec => ec).Distinct().ToList();

            // Check if votes were provided for all positions
            var missingPositions = new List<string>();

            foreach (var positionId in electionPositionIds)
            {
                if (!votesPerPosition.ContainsKey(positionId))
                {
                    var position = activePositions.FirstOrDefault(p => p.Id == positionId);
                    if (position != null)
                    {
                        missingPositions.Add(position.Name);
                    }
                }
            }

            if (missingPositions.Any())
            {
                result.Message = $"Debe seleccionar un candidato para los siguientes puestos: {string.Join(", ", missingPositions)}";
                result.MissingPositions = missingPositions;
                return result;
            }

            result.IsValid = true;
            result.Message = "ValidaciÃ³n completa exitosa.";
            return result;
        }

        private async Task<List<int>> GetElectionPositionsAsync(int electionId)
        {
            // This would get positions that have candidates in the election
            // For now, we'll get all active positions
            var positions = await _positionService.GetAll();
            return positions.Where(p => p.IsActive).Select(p => p.Id).ToList();
        }
    }
}
