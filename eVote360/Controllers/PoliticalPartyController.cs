using eVote360.Application.DTOs.PoliticalParty;
using eVote360.Application.Interfaces;
using eVote360.Application.ViewModels.PoliticalParty;
using eVote360.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    public class PoliticalPartyController : Controller
    {
        private readonly IPoliticalPartyService _service;
        private readonly IUserSession _userSession;
        private readonly IElectionService _electionService;

        public PoliticalPartyController(IPoliticalPartyService service, IUserSession userSession, IElectionService electionService)
        {
            _service = service;
            _userSession = userSession;
            _electionService = electionService;
        }

        #region Index
        public async Task<IActionResult> Index()
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var dtos = await _service.GetAll();

            var vms = dtos.Select(d => new PoliticalPartyViewModel
            {
                Id = d.Id,
                Name = d.Name,
                Acronym = d.Acronym,
                Description = d.Description,
                LogoUrl = d.LogoUrl,
                IsActive = d.IsActive
            }).ToList();

            return View(vms);
        }
        #endregion

        #region Create (GET/POST) - Vista compartida "Save"
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;


            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden crear partidos polï¿½ticos mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.EditMode = false;
            return View("Save", new
                SavePoliticalPartyViewModel
            {
                Acronym = "",
                Name = "",
                IsActive = true,
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(SavePoliticalPartyViewModel vm)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;


            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden crear partidos polï¿½ticos mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.EditMode = false;
                return View("Save", vm);
            }

            var dto = new PoliticalPartyDTO
            {
                Id = 0,
                Name = vm.Name,
                Acronym = vm.Acronym,
                Description = vm.Description,
                LogoUrl = null,
                IsActive = vm.IsActive
            };

            var ok = await _service.AddAsync(dto);
            if (!ok)
            {
                ModelState.AddModelError("", "An error occurred while saving the record.");
                ViewBag.EditMode = false;
                return View("Save", vm);
            }
            var all = await _service.GetAll();
            var created = all.FirstOrDefault(p => p.Acronym == vm.Acronym);
            if (created == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var file = Request.Form.Files["LogoFile"];
            if (file != null && file.Length > 0)
            {
                var newPath = FileManager.Upload(file, created.Id, "PoliticalParties");
                if (!string.IsNullOrWhiteSpace(newPath))
                {
                    created.LogoUrl = newPath;
                    await _service.UpdateAsync(created);
                }
            }

            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Edit (GET/POST) - Vista compartida "Save"
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;


            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden editar partidos polï¿½ticos mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            var dto = await _service.GetById(id);
            if (dto == null) return RedirectToAction(nameof(Index));

            var vm = new SavePoliticalPartyViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Acronym = dto.Acronym,
                Description = dto.Description,
                LogoUrl = dto.LogoUrl,
                IsActive = dto.IsActive
            };

            ViewBag.EditMode = true;
            return View("Save", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SavePoliticalPartyViewModel vm)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;


            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden editar partidos polï¿½ticos mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.EditMode = true;
                return View("Save", vm);
            }

            var current = await _service.GetById(vm.Id);
            string? currentLogoPath = current?.LogoUrl;

            var file = Request.Form.Files["LogoFile"];
            string? newLogoUrl = FileManager.Upload(file, vm.Id, "PoliticalParties", true, currentLogoPath);

            var dto = new PoliticalPartyDTO
            {
                Id = vm.Id,
                Name = vm.Name,
                Acronym = vm.Acronym,
                Description = vm.Description,
                LogoUrl = newLogoUrl,
                IsActive = vm.IsActive
            };

            var ok = await _service.UpdateAsync(dto);
            if (!ok)
            {
                ModelState.AddModelError("", "An error occurred while updating the record.");
                ViewBag.EditMode = true;
                return View("Save", vm);
            }

            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Activate / Deactivate (Confirm + Post)
        [HttpGet]
        public async Task<IActionResult> Activate(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var dto = await _service.GetById(id);
            if (dto == null)
                return NotFound();

            var vm = new ConfirmPoliticalPartyActionViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                IsActive = dto.IsActive
            };

            return View("ConfirmAction", vm);
        }

        [HttpPost]
        [ActionName("Activate")]
        public async Task<IActionResult> ActivateConfirmed(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;


            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden activar partidos polï¿½ticos mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            var success = await _service.ActivateAsync(id);

            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Political party activated successfully." : "Failed to activate political party.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Deactivate(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var dto = await _service.GetById(id);
            if (dto == null)
                return NotFound();

            var vm = new ConfirmPoliticalPartyActionViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                IsActive = dto.IsActive
            };

            return View("ConfirmAction", vm);
        }

        [HttpPost]
        [ActionName("Deactivate")]
        public async Task<IActionResult> DeactivateConfirmed(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;


            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden desactivar partidos polï¿½ticos mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            var success = await _service.DeactivateAsync(id);

            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Political party deactivated successfully." : "Failed to deactivate political party.";

            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Authorization Helper
        private IActionResult? CheckAuthorization()
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }

            return null; // Usuario autorizado
        }
        #endregion

    }
}
