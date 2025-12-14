using eVote360.Application.DTOs.User;
using eVote360.Application.Interfaces;
using eVote360.Application.ViewModels.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace eVote360.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPoliticalPartyService _partyService;
        private readonly IUserSession _userSession;
        private readonly IElectionService _electionService;

        public UserController(IUserService userService, IPoliticalPartyService partyService, IUserSession userSession, IElectionService electionService)
        {
            _userService = userService;
            _partyService = partyService;
            _userSession = userSession;
            _electionService = electionService;
        }

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

            var dtos = await _userService.GetAll();

            var list = dtos.Select(u => new UserViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Username = u.Username,
                Role = u.Role,
                IsActive = u.IsActive,
                PartyId = u.PartyId,
                PartyName = u.PartyName,
                PartyAcronym = u.PartyAcronym
            }).ToList();

            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }


            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden crear usuarios mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            await LoadSelectLists();
            var vm = new CreateUserViewModel()
            {
                Id = 0,
                FirstName = "",
                LastName = "",
                Email = "",
                IsActive = true,
                Username = "",
                Password = "",
                ConfirmPassword = "",
                Role = 0,
                PartyId = null
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel vm)
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }


            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden crear usuarios mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return View(vm);
            }

            if (await _userService.IsUsernameTaken(vm.Username))
            {
                TempData["ErrorMessage"] = "The username is already in use.";
                return RedirectToAction(nameof(Index));
            }

            var dto = new SaveUserDTO
            {
                Id = 0,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                IsActive = true,
                Username = vm.Username,
                PasswordHash = vm.Password,
                Role = vm.Role,
                PartyId = vm.PartyId
            };

            var created = await _userService.AddAsync(dto);

            TempData[created != null ? "SuccessMessage" : "ErrorMessage"] =
                created != null ? "User created successfully." : "Error creating user.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }


            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden editar usuarios mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            var dto = await _userService.GetById(id);
            if (dto == null) return RedirectToAction(nameof(Index));

            await LoadSelectLists();
            var vm = new UpdateUserViewModel()
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                IsActive = dto.IsActive,
                Username = dto.Username,
                Password = "",
                ConfirmPassword = "",
                Role = dto.Role,
                PartyId = dto.PartyId
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateUserViewModel vm)
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }


            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden editar usuarios mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return View(vm);
            }

            var dto = new SaveUserDTO
            {
                Id = vm.Id,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                IsActive = vm.IsActive,
                Email = vm.Email,
                Username = vm.Username,
                PasswordHash = vm.Password,
                Role = vm.Role,
                PartyId = vm.PartyId
            };

            var updated = await _userService.UpdateAsync(dto);
            TempData[updated != null ? "SuccessMessage" : "ErrorMessage"] =
                updated != null ? "User updated successfully." : "Error updating user.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Activate(int id)
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }

            var dto = await _userService.GetById(id);
            if (dto == null) return RedirectToAction(nameof(Index));

            return View("ConfirmAction", new ConfirmUserActionViewModel
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                IsActive = dto.IsActive
            });
        }

        [HttpPost]
        [ActionName("Activate")]
        public async Task<IActionResult> ActivateConfirmed(int id)
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }


            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden activar usuarios mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            var success = await _userService.ActivateAsync(id);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "User activated successfully." : "Unable to activate user.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Deactivate(int id)
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }

            var dto = await _userService.GetById(id);
            if (dto == null) return RedirectToAction(nameof(Index));

            return View("ConfirmAction", new ConfirmUserActionViewModel
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                IsActive = dto.IsActive
            });
        }

        [HttpPost]
        [ActionName("Deactivate")]
        public async Task<IActionResult> DeactivateConfirmed(int id)
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });
            }


            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden desactivar usuarios mientras hay una elecciï¿½n activa.";
                return RedirectToAction(nameof(Index));
            }

            var success = await _userService.DeactivateAsync(id);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "User deactivated successfully." : "Unable to deactivate user.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadSelectLists()
        {
            var parties = await _partyService.GetAll();
            ViewBag.Parties = new SelectList(parties.Select(p => new
            {
                Id = p.Id,
                Display = $"{p.Name} ({p.Acronym})"
            }), "Id", "Display");
        }

    }
}
