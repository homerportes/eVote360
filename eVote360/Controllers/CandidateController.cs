using eVote360.Application.DTOs.Candidate;
using eVote360.Application.DTOs.PoliticalParty;
using eVote360.Application.Interfaces;
using eVote360.Application.ViewModels.Candidate;
using eVote360.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace eVote360.Controllers
{
    public class CandidateController : Controller
    {
        private readonly ICandidateService _candidateService;
        private readonly ICitizenService _citizenService;
        private readonly IPoliticalPartyService _partyService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserSession _userSession;

        private readonly IElectionService _electionService;

        public CandidateController(
            ICandidateService candidateService,
            ICitizenService citizenService,
            IPoliticalPartyService partyService,
            IWebHostEnvironment webHostEnvironment,
            IUserSession userSession,
            IElectionService electionService)
        {
            _candidateService = candidateService;
            _citizenService = citizenService;
            _partyService = partyService;
            _webHostEnvironment = webHostEnvironment;
            _userSession = userSession;
            _electionService = electionService;
        }

        public async Task<IActionResult> Index()
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            List<CandidateDTO> dtos;
            var user = _userSession.GetUserSession();

            if (_userSession.IsAdmin())
            {
                dtos = await _candidateService.GetAll();
            }
            else if (_userSession.IsDirigente() && user?.PartyId.HasValue == true)
            {
                dtos = await _candidateService.GetByPartyId(user.PartyId.Value);
            }
            else
            {
                dtos = new List<CandidateDTO>();
            }

            var list = dtos.Select(c => new CandidateViewModel
            {
                Id = c.Id,
                CitizenId = c.CitizenId,
                PartyId = c.PartyId,
                PhotoUrl = c.PhotoUrl,
                IsActive = c.IsActive,
                CitizenFirstName = c.CitizenFirstName,
                CitizenLastName = c.CitizenLastName,
                PartyName = c.PartyName,
                PartyAcronym = c.PartyAcronym,
                ElectivePositionName = c.ElectivePositionName ?? "No position assigned"
            }).ToList();

            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden crear candidatos mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }

            await LoadSelectLists();

            var vm = new CreateCandidateViewModel()
            {
                Id = 0,
                CitizenId = 0,
                PartyId = 0,
                IsActive = true
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCandidateViewModel vm)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden crear candidatos mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }
            var user = _userSession.GetUserSession();
            if (_userSession.IsDirigente() && user?.PartyId.HasValue == true)
            {
                if (vm.PartyId != user.PartyId.Value)
                {
                    TempData["ErrorMessage"] = "You can only create candidates for your own political party.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return View(vm);
            }
            string? photoUrl = null;
            if (vm.Photo != null)
            {
                photoUrl = FileManager.Upload(vm.Photo, 0, "Candidates", false, "");
            }

            var dto = new SaveCandidateDTO
            {
                Id = 0,
                CitizenId = vm.CitizenId,
                PartyId = vm.PartyId,
                PhotoUrl = photoUrl,
                IsActive = true
            };

            var created = await _candidateService.AddAsync(dto);

            TempData[created != null ? "SuccessMessage" : "ErrorMessage"] =
                created != null ? "Candidate created successfully." : "Error creating candidate.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden editar candidatos mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }

            var dto = await _candidateService.GetById(id);
            if (dto == null) return RedirectToAction(nameof(Index));
            var user = _userSession.GetUserSession();
            if (_userSession.IsDirigente() && user?.PartyId.HasValue == true)
            {
                if (dto.PartyId != user.PartyId.Value)
                {
                    TempData["ErrorMessage"] = "You can only edit candidates from your own political party.";
                    return RedirectToAction(nameof(Index));
                }
            }

            await LoadSelectLists();

            var vm = new UpdateCandidateViewModel()
            {
                Id = dto.Id,
                CitizenId = dto.CitizenId,
                PartyId = dto.PartyId,
                CurrentPhotoUrl = dto.PhotoUrl,
                IsActive = dto.IsActive
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateCandidateViewModel vm)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden editar candidatos mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }
            var user = _userSession.GetUserSession();
            if (_userSession.IsDirigente() && user?.PartyId.HasValue == true)
            {
                if (vm.PartyId != user.PartyId.Value)
                {
                    TempData["ErrorMessage"] = "You can only edit candidates from your own political party.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return View(vm);
            }
            string? photoUrl = vm.CurrentPhotoUrl;
            if (vm.Photo != null)
            {
                photoUrl = FileManager.Upload(vm.Photo, vm.Id, "Candidates", true, vm.CurrentPhotoUrl);
            }

            var dto = new SaveCandidateDTO
            {
                Id = vm.Id,
                CitizenId = vm.CitizenId,
                PartyId = vm.PartyId,
                PhotoUrl = photoUrl,
                IsActive = vm.IsActive
            };

            var updated = await _candidateService.UpdateAsync(dto);

            TempData[updated ? "SuccessMessage" : "ErrorMessage"] =
                updated ? "Candidate updated successfully." : "Error updating candidate.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Activate(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var dto = await _candidateService.GetById(id);
            if (dto == null) return RedirectToAction(nameof(Index));
            var user = _userSession.GetUserSession();
            if (_userSession.IsDirigente() && user?.PartyId.HasValue == true)
            {
                if (dto.PartyId != user.PartyId.Value)
                {
                    TempData["ErrorMessage"] = "You can only activate candidates from your own political party.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View("ConfirmAction", new ConfirmCandidateActionViewModel
            {
                Id = dto.Id,
                CitizenFirstName = dto.CitizenFirstName,
                CitizenLastName = dto.CitizenLastName,
                PhotoUrl = dto.PhotoUrl,
                IsActive = dto.IsActive
            });
        }

        [HttpPost]
        [ActionName("Activate")]
        public async Task<IActionResult> ActivateConfirmed(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden activar candidatos mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }
            var dto = await _candidateService.GetById(id);
            var user = _userSession.GetUserSession();
            if (_userSession.IsDirigente() && user?.PartyId.HasValue == true && dto != null)
            {
                if (dto.PartyId != user.PartyId.Value)
                {
                    TempData["ErrorMessage"] = "You can only activate candidates from your own political party.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var success = await _candidateService.ActivateAsync(id);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Candidate activated successfully." : "Unable to activate candidate.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Deactivate(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var dto = await _candidateService.GetById(id);
            if (dto == null) return RedirectToAction(nameof(Index));
            var user = _userSession.GetUserSession();
            if (_userSession.IsDirigente() && user?.PartyId.HasValue == true)
            {
                if (dto.PartyId != user.PartyId.Value)
                {
                    TempData["ErrorMessage"] = "You can only deactivate candidates from your own political party.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View("ConfirmAction", new ConfirmCandidateActionViewModel
            {
                Id = dto.Id,
                CitizenFirstName = dto.CitizenFirstName,
                CitizenLastName = dto.CitizenLastName,
                PhotoUrl = dto.PhotoUrl,
                IsActive = dto.IsActive
            });
        }

        [HttpPost]
        [ActionName("Deactivate")]
        public async Task<IActionResult> DeactivateConfirmed(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden desactivar candidatos mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }
            var dto = await _candidateService.GetById(id);
            var user = _userSession.GetUserSession();
            if (_userSession.IsDirigente() && user?.PartyId.HasValue == true && dto != null)
            {
                if (dto.PartyId != user.PartyId.Value)
                {
                    TempData["ErrorMessage"] = "You can only deactivate candidates from your own political party.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var success = await _candidateService.DeactivateAsync(id);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Candidate deactivated successfully." : "Unable to deactivate candidate.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadSelectLists()
        {
            var citizens = await _citizenService.GetAll();
            var user = _userSession.GetUserSession();
            List<PoliticalPartyDTO> parties;
            if (_userSession.IsDirigente() && user?.PartyId.HasValue == true)
            {
                var party = await _partyService.GetById(user.PartyId.Value);
                parties = party != null ? new List<PoliticalPartyDTO> { party } : new List<PoliticalPartyDTO>();
            }
            else
            {
                parties = await _partyService.GetAll();
            }

            ViewBag.Citizens = new SelectList(
                citizens.Where(c => c.IsActive).Select(c => new
                {
                    Id = c.Id,
                    FullName = $"{c.FirstName} {c.LastName} - {c.IdentityDocument}"
                }),
                "Id",
                "FullName"
            );

            ViewBag.Parties = new SelectList(
                parties.Where(p => p.IsActive).Select(p => new
                {
                    Id = p.Id,
                    Display = $"{p.Name} ({p.Acronym})"
                }),
                "Id",
                "Display"
            );
        }

        private IActionResult? CheckAuthorization()
        {
            if (!_userSession.HasUser())
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            if (!_userSession.IsAdmin() && !_userSession.IsDirigente())
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });

            return null;
        }
    }
}
