using eVote360.Application.DTOs.ElectivePosition;
using eVote360.Application.Interfaces;
using eVote360.Application.ViewModels.ElectivePosition;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    public class ElectivePositionController : Controller
    {
        private readonly IElectivePositionService _service;
        private readonly IUserSession _userSession;
        private readonly IElectionService _electionService;

        public ElectivePositionController(IElectivePositionService service, IUserSession userSession, IElectionService electionService)
        {
            _service = service;
            _userSession = userSession;
            _electionService = electionService;
        }

        #region Index
        public async Task<IActionResult> Index()
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }

            try
            {
                var listDtos = await _service.GetAll();

                var viewModelList = listDtos.Select(e => new ElectivePositionViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    IsActive = e.IsActive
                }).ToList();

                return View(viewModelList);
            }
            catch (Exception)
            {
                return View(new List<ElectivePositionViewModel>());
            }
        }
        #endregion

        #region Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;


            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden crear puestos electivos mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.EditMode = false;
            return View("Save", new SaveElectivePositionViewModel
            {
                Name = string.Empty,
                Description = string.Empty,
                IsActive = true
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveElectivePositionViewModel vm)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;


            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden crear puestos electivos mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.EditMode = false;
                return View("Save", vm);
            }

            ElectivePositionDTO dto = new()
            {
                Id = 0,
                Name = vm.Name,
                Description = vm.Description,
                IsActive = vm.IsActive
            };

            var result = await _service.AddAsync(dto);

            if (!result)
            {
                ModelState.AddModelError("", "An error occurred while saving the record.");
                ViewBag.EditMode = false;
                return View("Save", vm);
            }

            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;


            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden editar puestos electivos mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            var dto = await _service.GetById(id);
            if (dto == null)
                return RedirectToAction(nameof(Index));

            SaveElectivePositionViewModel vm = new()
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive
            };

            ViewBag.EditMode = true;
            return View("Save", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SaveElectivePositionViewModel vm)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;


            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden editar puestos electivos mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.EditMode = true;
                return View("Save", vm);
            }

            ElectivePositionDTO dto = new()
            {
                Id = vm.Id,
                Name = vm.Name,
                Description = vm.Description,
                IsActive = vm.IsActive
            };

            var result = await _service.UpdateAsync(dto);

            if (!result)
            {
                ModelState.AddModelError("", "An error occurred while updating the record.");
                ViewBag.EditMode = true;
                return View("Save", vm);
            }

            return RedirectToAction(nameof(Index));
        }
        #endregion
        public async Task<IActionResult> Activate(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var dto = await _service.GetById(id);
            if (dto == null) return NotFound();

            return View("ConfirmAction", new ConfirmElectivePositionActionViewModel
            {
                Id = id,
                Name = dto.Name,
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
                TempData["ErrorMessage"] = "No se pueden activar puestos electivos mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            var success = await _service.ActivateAsync(id);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Position successfully activated." : "Failed to activate the position.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Deactivate(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var dto = await _service.GetById(id);
            if (dto == null) return NotFound();

            return View("ConfirmAction", new ConfirmElectivePositionActionViewModel
            {
                Id = id,
                Name = dto.Name,
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
                TempData["ErrorMessage"] = "No se pueden desactivar puestos electivos mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            var success = await _service.DeactivateAsync(id);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Position successfully deactivated." : "Failed to deactivate the position.";
            return RedirectToAction(nameof(Index));
        }

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
