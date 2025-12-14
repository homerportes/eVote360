using eVote360.Application.DTOs.Election;
using eVote360.Application.Interfaces;
using eVote360.Domain.Common.Enums;
using eVote360.Domain.Entities;
using eVote360.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Application.Services
{
    public class ElectionService : IElectionService
    {
        private readonly IElectionRepository _electionRepository;
        private readonly IElectionCandidacyRepository _electionCandidacyRepository;
        private readonly IElectivePositionRepository _positionRepository;
        private readonly IPoliticalPartyRepository _partyRepository;
        private readonly ICandidacyRepository _candidacyRepository;
        private readonly IVoteRepository _voteRepository;

        public ElectionService(
            IElectionRepository electionRepository,
            IElectionCandidacyRepository electionCandidacyRepository,
            IElectivePositionRepository positionRepository,
            IPoliticalPartyRepository partyRepository,
            ICandidacyRepository candidacyRepository,
            IVoteRepository voteRepository)
        {
            _electionRepository = electionRepository;
            _electionCandidacyRepository = electionCandidacyRepository;
            _positionRepository = positionRepository;
            _partyRepository = partyRepository;
            _candidacyRepository = candidacyRepository;
            _voteRepository = voteRepository;
        }

        public async Task<ElectionDTO?> CreateElectionAsync(SaveElectionDTO dto)
        {
            // Validate before creating
            var validation = await ValidateElectionCreationAsync();
            if (!validation.IsValid)
            {
                return null;
            }

            // Create election with InProcess status
            var election = new Election
            {
                Name = dto.Name,
                Date = dto.Date,
                Status = ElectionStatus.InProcess
            };

            var created = await _electionRepository.AddAsync(election);
            if (created == null) return null;

            // Get all active elective positions
            var activePositions = await _positionRepository.GetAllList();
            var positions = activePositions.Where(p => p.IsActive).ToList();

            // Get all candidacies for active positions
            var allCandidacies = await _candidacyRepository.GetAllList();
            var candidaciesForActivePositions = allCandidacies
                .Where(c => positions.Any(p => p.Id == c.ElectivePositionId))
                .ToList();

            // Create ElectionCandidacy entries
            foreach (var candidacy in candidaciesForActivePositions)
            {
                await _electionCandidacyRepository.AddAsync(new ElectionCandidacy
                {
                    ElectionId = created.Id,
                    CandidacyId = candidacy.Id
                });
            }

            return await GetElectionDTOAsync(created.Id);
        }

        public async Task<List<ElectionDTO>> GetAllElectionsAsync()
        {
            var elections = await _electionRepository.GetAllList();
            var electionDTOs = new List<ElectionDTO>();

            foreach (var election in elections.OrderByDescending(e => e.Date))
            {
                var dto = await GetElectionDTOAsync(election.Id);
                if (dto != null)
                {
                    electionDTOs.Add(dto);
                }
            }

            // Sort: Active first, then by date descending
            return electionDTOs
                .OrderByDescending(e => e.Status == ElectionStatus.InProcess)
                .ThenByDescending(e => e.Date)
                .ToList();
        }

        public async Task<ElectionDTO?> GetElectionByIdAsync(int electionId)
        {
            return await GetElectionDTOAsync(electionId);
        }

        public async Task<ElectionDTO?> GetActiveElectionAsync()
        {
            var elections = await _electionRepository.GetAllList();
            var activeElection = elections.FirstOrDefault(e => e.Status == ElectionStatus.InProcess);

            if (activeElection == null) return null;

            return await GetElectionDTOAsync(activeElection.Id);
        }

        public async Task<bool> HasActiveElectionAsync()
        {
            var elections = await _electionRepository.GetAllList();
            return elections.Any(e => e.Status == ElectionStatus.InProcess);
        }

        public async Task<bool> FinishElectionAsync(int electionId)
        {
            var election = await _electionRepository.GetById(electionId);
            if (election == null || election.Status != ElectionStatus.InProcess)
            {
                return false;
            }

            election.Status = ElectionStatus.Finished;
            var updated = await _electionRepository.UpdateAsync(electionId, election);
            return updated != null;
        }

        public async Task<ElectionResultDTO?> GetElectionResultsAsync(int electionId)
        {
            var election = await _electionRepository.GetById(electionId);
            if (election == null || election.Status != ElectionStatus.Finished)
            {
                return null;
            }

            var result = new ElectionResultDTO
            {
                ElectionId = election.Id,
                ElectionName = election.Name,
                ElectionDate = election.Date
            };

            // Get all election candidacies
            var allElectionCandidacies = await _electionCandidacyRepository.GetAllList();
            var electionCandidacies = allElectionCandidacies
                .Where(ec => ec.ElectionId == electionId)
                .ToList();

            // Get all candidacies with their related data
            var allCandidacies = await _candidacyRepository.GetAllList();
            var candidacies = allCandidacies
                .Where(c => electionCandidacies.Any(ec => ec.CandidacyId == c.Id))
                .ToList();

            // Get all votes for this election
            var allVotes = await _voteRepository.GetAllList();
            var electionVotes = allVotes.Where(v => v.ElectionId == electionId).ToList();

            // Group by position
            var positionGroups = candidacies.GroupBy(c => c.ElectivePositionId);

            foreach (var positionGroup in positionGroups)
            {
                var positionId = positionGroup.Key;
                var position = await _positionRepository.GetById(positionId);
                if (position == null) continue;

                // Get votes for this position
                var positionVotes = electionVotes.Where(v => v.ElectivePositionId == positionId).ToList();
                var totalVotes = positionVotes.Count;

                var positionResult = new PositionResultDTO
                {
                    PositionId = position.Id,
                    PositionName = position.Name,
                    TotalVotes = totalVotes
                };

                // Get results for each candidate
                foreach (var candidacy in positionGroup)
                {
                    var votesForCandidate = positionVotes.Count(v => v.CandidateId == candidacy.CandidateId);
                    var percentage = totalVotes > 0 ? (decimal)votesForCandidate / totalVotes * 100 : 0;

                    var candidateResult = new CandidateResultDTO
                    {
                        CandidateId = candidacy.CandidateId,
                        CandidateName = $"{candidacy.Candidate?.Citizen?.FirstName} {candidacy.Candidate?.Citizen?.LastName}",
                        PartyAcronym = candidacy.PostulatingParty?.Acronym ?? "",
                        PartyName = candidacy.PostulatingParty?.Name ?? "",
                        VotesReceived = votesForCandidate,
                        VotePercentage = Math.Round(percentage, 2)
                    };

                    positionResult.CandidateResults.Add(candidateResult);
                }

                // Add blank votes (where CandidateId is null)
                var blankVotes = positionVotes.Count(v => v.CandidateId == null);
                if (blankVotes > 0)
                {
                    var blankPercentage = totalVotes > 0 ? (decimal)blankVotes / totalVotes * 100 : 0;
                    var blankVoteResult = new CandidateResultDTO
                    {
                        CandidateId = 0, // Sentinel value for blank vote
                        CandidateName = "Ninguno",
                        PartyAcronym = "Voto en Blanco",
                        PartyName = "Voto en Blanco",
                        VotesReceived = blankVotes,
                        VotePercentage = Math.Round(blankPercentage, 2)
                    };
                    positionResult.CandidateResults.Add(blankVoteResult);
                }

                // Sort candidates by votes descending (winner first)
                positionResult.CandidateResults = positionResult.CandidateResults
                    .OrderByDescending(cr => cr.VotesReceived)
                    .ToList();

                result.PositionResults.Add(positionResult);
            }

            return result;
        }

        public async Task<ElectionValidationResultDTO> ValidateElectionCreationAsync()
        {
            var result = new ElectionValidationResultDTO
            {
                IsValid = true,
                HasSufficientParties = true,
                AllPartiesHaveCandidatesForAllPositions = true
            };

            // Check if there's at least one active elective position
            var allPositions = await _positionRepository.GetAllList();
            var activePositions = allPositions.Where(p => p.IsActive).ToList();

            if (!activePositions.Any())
            {
                result.IsValid = false;
                result.ValidationMessages.Add("No hay puestos electivos activos para realizar una elecciÃ³n.");
                return result;
            }

            // Check if there are at least 2 active political parties
            var allParties = await _partyRepository.GetAllList();
            var activeParties = allParties.Where(p => p.IsActive).ToList();

            if (activeParties.Count < 2)
            {
                result.IsValid = false;
                result.HasSufficientParties = false;
                result.ValidationMessages.Add("No hay suficientes partidos polÃ­ticos para realizar una elecciÃ³n.");
                return result;
            }

            // Check if each party has candidates for all active positions
            var allCandidacies = await _candidacyRepository.GetAllList();

            foreach (var party in activeParties)
            {
                var partyCandidacies = allCandidacies
                    .Where(c => c.PostulatingPartyId == party.Id)
                    .ToList();

                var positionsWithCandidates = partyCandidacies
                    .Select(c => c.ElectivePositionId)
                    .Distinct()
                    .ToList();

                var missingPositions = activePositions
                    .Where(p => !positionsWithCandidates.Contains(p.Id))
                    .ToList();

                if (missingPositions.Any())
                {
                    result.IsValid = false;
                    result.AllPartiesHaveCandidatesForAllPositions = false;

                    var positionNames = string.Join(", ", missingPositions.Select(p => p.Name));
                    result.ValidationMessages.Add(
                        $"El partido polÃ­tico {party.Name} [{party.Acronym}] no tiene candidatos registrados para los siguientes puestos electivos: {positionNames}.");
                }
            }

            return result;
        }

        public async Task<List<int>> GetAvailableElectionYearsAsync()
        {
            var elections = await _electionRepository.GetAllList();

            var years = elections
                .Select(e => e.Date.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            return years;
        }

        public async Task<List<ElectionSummaryDTO>> GetElectionSummariesByYearAsync(int year)
        {
            var elections = await _electionRepository.GetAllList();
            var yearElections = elections.Where(e => e.Date.Year == year).ToList();

            var summaries = new List<ElectionSummaryDTO>();

            foreach (var election in yearElections)
            {
                // Get election candidacies
                var allElectionCandidacies = await _electionCandidacyRepository.GetAllList();
                var electionCandidacies = allElectionCandidacies
                    .Where(ec => ec.ElectionId == election.Id)
                    .ToList();

                // Get candidacies to determine parties and candidates
                var allCandidacies = await _candidacyRepository.GetAllList();
                var candidacies = allCandidacies
                    .Where(c => electionCandidacies.Any(ec => ec.CandidacyId == c.Id))
                    .ToList();

                var totalParties = candidacies
                    .Select(c => c.PostulatingPartyId)
                    .Distinct()
                    .Count();

                var totalCandidates = candidacies
                    .Select(c => c.CandidateId)
                    .Distinct()
                    .Count();

                // Get total votes (unique citizens who voted)
                var allVotes = await _voteRepository.GetAllList();
                var totalVotes = allVotes
                    .Where(v => v.ElectionId == election.Id)
                    .Select(v => v.CitizenId)
                    .Distinct()
                    .Count();

                summaries.Add(new ElectionSummaryDTO
                {
                    ElectionId = election.Id,
                    ElectionName = election.Name,
                    ElectionDate = election.Date,
                    TotalParties = totalParties,
                    TotalCandidates = totalCandidates,
                    TotalVotes = totalVotes
                });
            }

            return summaries.OrderByDescending(s => s.ElectionDate).ToList();
        }

        private async Task<ElectionDTO?> GetElectionDTOAsync(int electionId)
        {
            var election = await _electionRepository.GetById(electionId);
            if (election == null) return null;

            // Get election candidacies
            var allElectionCandidacies = await _electionCandidacyRepository.GetAllList();
            var electionCandidacies = allElectionCandidacies
                .Where(ec => ec.ElectionId == electionId)
                .ToList();

            // Get candidacies to determine parties and positions
            var allCandidacies = await _candidacyRepository.GetAllList();
            var candidacies = allCandidacies
                .Where(c => electionCandidacies.Any(ec => ec.CandidacyId == c.Id))
                .ToList();

            var participatingParties = candidacies
                .Select(c => c.PostulatingPartyId)
                .Distinct()
                .Count();

            var disputedPositions = candidacies
                .Select(c => c.ElectivePositionId)
                .Distinct()
                .Count();

            return new ElectionDTO
            {
                Id = election.Id,
                Name = election.Name,
                Date = election.Date,
                Status = election.Status,
                ParticipatingPartiesCount = participatingParties,
                DisputedPositionsCount = disputedPositions
            };
        }
    }
}
