using eVote360.Application.DTOs.Citizen;
using eVote360.Application.Interfaces;
using eVote360.Application.ViewModels.Citizen;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    public class CitizenController : Controller
    {
        private readonly ICitizenService _citizenService;
        private readonly IUserSession _userSession;
        private readonly IElectionService _electionService;

        public CitizenController(ICitizenService citizenService, IUserSession userSession, IElectionService electionService)
        {
            _citizenService = citizenService;
            _userSession = userSession;
            _electionService = electionService;
        }

        public async Task<IActionResult> Index()
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var dtos = await _citizenService.GetAll();

            var list = dtos.Select(c => new CitizenViewModel
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                IdentityDocument = c.IdentityDocument,
                IsActive = c.IsActive
            }).ToList();

            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden crear ciudadanos mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new CreateCitizenViewModel()
            {
                Id = 0,
                FirstName = "",
                LastName = "",
                Email = "",
                IdentityDocument = "",
                IsActive = true
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCitizenViewModel vm)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden crear ciudadanos mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            if (await _citizenService.IsIdentityDocumentTaken(vm.IdentityDocument))
            {
                ModelState.AddModelError("IdentityDocument", "This identity document is already registered.");
                return View(vm);
            }

            var dto = new SaveCitizenDTO
            {
                Id = 0,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                IdentityDocument = vm.IdentityDocument,
                IsActive = true
            };

            var created = await _citizenService.AddAsync(dto);

            TempData[created != null ? "SuccessMessage" : "ErrorMessage"] =
                created != null ? "Citizen created successfully." : "Error creating citizen.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden editar ciudadanos mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }

            var dto = await _citizenService.GetById(id);
            if (dto == null) return RedirectToAction(nameof(Index));

            var vm = new UpdateCitizenViewModel()
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                IdentityDocument = dto.IdentityDocument,
                IsActive = dto.IsActive
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateCitizenViewModel vm)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden editar ciudadanos mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            if (await _citizenService.IsIdentityDocumentTaken(vm.IdentityDocument, vm.Id))
            {
                ModelState.AddModelError("IdentityDocument", "This identity document is already registered.");
                return View(vm);
            }

            var dto = new SaveCitizenDTO
            {
                Id = vm.Id,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                IdentityDocument = vm.IdentityDocument,
                IsActive = vm.IsActive
            };

            var updated = await _citizenService.UpdateAsync(dto);
            TempData[updated ? "SuccessMessage" : "ErrorMessage"] =
                updated ? "Citizen updated successfully." : "Error updating citizen.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Activate(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var dto = await _citizenService.GetById(id);
            if (dto == null) return RedirectToAction(nameof(Index));

            return View("ConfirmAction", new ConfirmCitizenActionViewModel
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                IdentityDocument = dto.IdentityDocument,
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
                TempData["ErrorMessage"] = "No se pueden activar ciudadanos mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }

            var success = await _citizenService.ActivateAsync(id);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Citizen activated successfully." : "Unable to activate citizen.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Deactivate(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var dto = await _citizenService.GetById(id);
            if (dto == null) return RedirectToAction(nameof(Index));

            return View("ConfirmAction", new ConfirmCitizenActionViewModel
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                IdentityDocument = dto.IdentityDocument,
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
                TempData["ErrorMessage"] = "No se pueden desactivar ciudadanos mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }

            var success = await _citizenService.DeactivateAsync(id);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Citizen deactivated successfully." : "Unable to deactivate citizen.";
            return RedirectToAction(nameof(Index));
        }

        #region Authorization Helper
        private IActionResult? CheckAuthorization()
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin() && !_userSession.IsDirigente())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }

            return null; // Usuario autorizado
        }
        #endregion
    }
}
