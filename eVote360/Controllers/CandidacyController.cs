using eVote360.Application.DTOs.Candidacy;
using eVote360.Application.Interfaces;
using eVote360.Application.ViewModels.Candidacy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace eVote360.Controllers
{
    public class CandidacyController : Controller
    {
        private readonly ICandidacyService _candidacyService;
        private readonly ICandidateService _candidateService;
        private readonly IElectivePositionService _positionService;
        private readonly IPoliticalPartyService _partyService;
        private readonly IUserSession _userSession;
        private readonly IElectionService _electionService;

        public CandidacyController(
            ICandidacyService candidacyService,
            ICandidateService candidateService,
            IElectivePositionService positionService,
            IPoliticalPartyService partyService,
            IUserSession userSession,
            IElectionService electionService)
        {
            _candidacyService = candidacyService;
            _candidateService = candidateService;
            _positionService = positionService;
            _partyService = partyService;
            _userSession = userSession;
            _electionService = electionService;
        }

        public async Task<IActionResult> Index()
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var user = _userSession.GetUserSession();
            if (user?.PartyId == null)
            {
                TempData["ErrorMessage"] = "You must be assigned to a political party to manage candidacies.";
                return RedirectToRoute(new { controller = "DirigenteHome", action = "Index" });
            }
            var candidacies = await _candidacyService.GetByPartyId(user.PartyId.Value);

            var viewModels = candidacies.Select(c => new CandidacyViewModel
            {
                Id = c.Id,
                CandidateFullName = $"{c.CandidateFirstName} {c.CandidateLastName}",
                ElectivePositionName = c.ElectivePositionName,
                IsAlliance = c.IsAlliance
            }).ToList();

            return View(viewModels);
        }

        public async Task<IActionResult> Create()
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var user = _userSession.GetUserSession();
            if (user?.PartyId == null)
            {
                TempData["ErrorMessage"] = "You must be assigned to a political party.";
                return RedirectToAction(nameof(Index));
            }

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden crear candidaturas mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }

            await LoadSelectLists(user.PartyId.Value);

            var vm = new CreateCandidacyViewModel
            {
                CandidateId = 0,
                ElectivePositionId = 0
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCandidacyViewModel vm)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var user = _userSession.GetUserSession();
            if (user?.PartyId == null)
            {
                TempData["ErrorMessage"] = "You must be assigned to a political party.";
                return RedirectToAction(nameof(Index));
            }

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden crear candidaturas mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectLists(user.PartyId.Value);
                return View(vm);
            }
            var existingCandidacy = await _candidacyService.ExistsCandidacyForPartyAndPosition(user.PartyId.Value, vm.ElectivePositionId);
            if (existingCandidacy)
            {
                TempData["ErrorMessage"] = "Your party already has a candidate assigned to this elective position.";
                await LoadSelectLists(user.PartyId.Value);
                return View(vm);
            }
            var candidateExistingCandidacy = await _candidacyService.GetCandidacyByPartyAndCandidate(user.PartyId.Value, vm.CandidateId);
            if (candidateExistingCandidacy != null)
            {
                TempData["ErrorMessage"] = "This candidate is already assigned to a position in your party.";
                await LoadSelectLists(user.PartyId.Value);
                return View(vm);
            }
            var candidate = await _candidateService.GetById(vm.CandidateId);
            if (candidate == null)
            {
                TempData["ErrorMessage"] = "Candidate not found.";
                return RedirectToAction(nameof(Index));
            }

            bool isAlliance = candidate.PartyId != user.PartyId.Value;
            if (isAlliance)
            {
                var candidateCandidacies = await _candidacyService.GetByCandidateId(vm.CandidateId);
                var originalCandidacy = candidateCandidacies.FirstOrDefault(c => c.PostulatingPartyId == candidate.PartyId);

                if (originalCandidacy != null && originalCandidacy.ElectivePositionId != vm.ElectivePositionId)
                {
                    TempData["ErrorMessage"] = "This candidate from an allied party is running for a different position in their original party. Candidates can only run for the same position across parties.";
                    await LoadSelectLists(user.PartyId.Value);
                    return View(vm);
                }
            }

            var dto = new SaveCandidacyDTO
            {
                Id = 0,
                CandidateId = vm.CandidateId,
                ElectivePositionId = vm.ElectivePositionId,
                PostulatingPartyId = user.PartyId.Value,
                IsAlliance = isAlliance
            };

            var created = await _candidacyService.AddAsync(dto);

            TempData[created != null ? "SuccessMessage" : "ErrorMessage"] =
                created != null ? "Candidacy created successfully." : "Error creating candidacy.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var user = _userSession.GetUserSession();
            if (user?.PartyId == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden eliminar candidaturas mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }

            var candidacy = await _candidacyService.GetById(id);
            if (candidacy == null || candidacy.PostulatingPartyId != user.PartyId.Value)
            {
                return RedirectToAction(nameof(Index));
            }

            var vm = new DeleteCandidacyViewModel
            {
                Id = candidacy.Id,
                CandidateFullName = $"{candidacy.CandidateFirstName} {candidacy.CandidateLastName}",
                ElectivePositionName = candidacy.ElectivePositionName
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var user = _userSession.GetUserSession();
            if (user?.PartyId == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden eliminar candidaturas mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }
            var candidacy = await _candidacyService.GetById(id);
            if (candidacy == null || candidacy.PostulatingPartyId != user.PartyId.Value)
            {
                TempData["ErrorMessage"] = "Candidacy not found or does not belong to your party.";
                return RedirectToAction(nameof(Index));
            }

            var deleted = await _candidacyService.DeleteAsync(id);

            TempData[deleted ? "SuccessMessage" : "ErrorMessage"] =
                deleted ? "Candidacy deleted successfully." : "Error deleting candidacy.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadSelectLists(int partyId)
        {
            var ownPartyCandidates = await _candidateService.GetByPartyId(partyId);
            var activeCandidates = ownPartyCandidates.Where(c => c.IsActive).ToList();
            var existingCandidacies = await _candidacyService.GetByPartyId(partyId);
            var candidatesWithCandidacy = existingCandidacies.Select(c => c.CandidateId).ToHashSet();
            var availableCandidates = activeCandidates
                .Where(c => !candidatesWithCandidacy.Contains(c.Id))
                .Select(c => new
                {
                    Id = c.Id,
                    FullName = $"{c.CitizenFirstName} {c.CitizenLastName}"
                })
                .ToList();

            ViewBag.Candidates = new SelectList(availableCandidates, "Id", "FullName");
            var allPositions = await _positionService.GetAll();
            var occupiedPositions = existingCandidacies.Select(c => c.ElectivePositionId).ToHashSet();

            var availablePositions = allPositions
                .Where(p => p.IsActive && !occupiedPositions.Contains(p.Id))
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToList();

            ViewBag.Positions = new SelectList(availablePositions, "Id", "Name");
        }

        private IActionResult? CheckAuthorization()
        {
            if (!_userSession.HasUser())
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            if (!_userSession.IsDirigente())
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });

            return null;
        }
    }
}
