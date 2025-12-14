using eVote360.Application.Interfaces;
using eVote360.Application.ViewModels.PartyLeaderAssignment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace eVote360.Controllers
{
    public class PartyLeaderAssignmentController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPoliticalPartyService _partyService;
        private readonly IUserSession _userSession;
        private readonly IElectionService _electionService;

        public PartyLeaderAssignmentController(
            IUserService userService,
            IPoliticalPartyService partyService,
            IUserSession userSession,
            IElectionService electionService)
        {
            _userService = userService;
            _partyService = partyService;
            _userSession = userSession;
            _electionService = electionService;
        }

        public async Task<IActionResult> Index()
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;
            var allUsers = await _userService.GetAll();
            var assignedLeaders = allUsers
                .Where(u => u.Role == (int)eVote360.Domain.Common.Enums.UserRole.PoliticalLeader && u.PartyId.HasValue)
                .Select(u => new PartyLeaderAssignmentViewModel
                {
                    UserId = u.Id,
                    UserFullName = $"{u.FirstName} {u.LastName}",
                    PartyId = u.PartyId!.Value,
                    PartyAcronym = u.PartyAcronym ?? "N/A"
                })
                .ToList();

            return View(assignedLeaders);
        }

        public async Task<IActionResult> Create()
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden crear asignaciones de dirigentes mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }

            await LoadSelectLists();

            var vm = new CreatePartyLeaderAssignmentViewModel
            {
                UserId = 0,
                PartyId = 0
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePartyLeaderAssignmentViewModel vm)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden crear asignaciones de dirigentes mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return View(vm);
            }
            var user = await _userService.GetById(vm.UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            if (user.Role != (int)eVote360.Domain.Common.Enums.UserRole.PoliticalLeader)
            {
                TempData["ErrorMessage"] = "Only users with Political Leader role can be assigned to a party.";
                return RedirectToAction(nameof(Index));
            }
            if (user.PartyId.HasValue)
            {
                TempData["ErrorMessage"] = "This political leader is already assigned to another party.";
                return RedirectToAction(nameof(Index));
            }
            var updated = await _userService.UpdatePartyAssignmentAsync(vm.UserId, vm.PartyId);

            TempData[updated ? "SuccessMessage" : "ErrorMessage"] =
                updated ? "Political leader assigned to party successfully." : "Error assigning political leader to party.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden eliminar asignaciones de dirigentes mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userService.GetById(id);
            if (user == null || !user.PartyId.HasValue)
            {
                return RedirectToAction(nameof(Index));
            }

            var vm = new DeletePartyLeaderAssignmentViewModel
            {
                UserId = user.Id,
                UserFullName = $"{user.FirstName} {user.LastName}",
                PartyName = user.PartyName ?? "Unknown",
                PartyAcronym = user.PartyAcronym ?? ""
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            if (await _electionService.HasActiveElectionAsync())
            {
                TempData["ErrorMessage"] = "No se pueden eliminar asignaciones de dirigentes mientras hay una elecciÃ³n activa.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userService.GetById(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            var updated = await _userService.UpdatePartyAssignmentAsync(id, null);

            TempData[updated ? "SuccessMessage" : "ErrorMessage"] =
                updated ? "Political leader unassigned from party successfully." : "Error unassigning political leader from party.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadSelectLists()
        {
            var allUsers = await _userService.GetAll();
            var parties = await _partyService.GetAll();
            var availableLeaders = allUsers
                .Where(u => u.Role == (int)eVote360.Domain.Common.Enums.UserRole.PoliticalLeader
                         && u.IsActive
                         && !u.PartyId.HasValue)
                .Select(u => new
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName} - {u.Username}"
                })
                .ToList();

            ViewBag.Users = new SelectList(availableLeaders, "Id", "FullName");
            ViewBag.Parties = new SelectList(
                parties.Where(p => p.IsActive).Select(p => new
                {
                    Id = p.Id,
                    Display = $"{p.Acronym}"
                }),
                "Id",
                "Display"
            );
        }

        private IActionResult? CheckAuthorization()
        {
            if (!_userSession.HasUser())
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            if (!_userSession.IsAdmin())
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });

            return null;
        }
    }
}
