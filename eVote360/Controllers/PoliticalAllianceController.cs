using eVote360.Application.DTOs.PoliticalAlliance;
using eVote360.Application.Interfaces;
using eVote360.Application.ViewModels.PoliticalAlliance;
using eVote360.Domain.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    public class PoliticalAllianceController : Controller
    {
        private readonly IPoliticalAllianceService _allianceService;
        private readonly IPoliticalPartyService _partyService;
        private readonly IUserSession _userSession;

        public PoliticalAllianceController(
            IPoliticalAllianceService allianceService,
            IPoliticalPartyService partyService,
            IUserSession userSession)
        {
            _allianceService = allianceService;
            _partyService = partyService;
            _userSession = userSession;
        }

        public async Task<IActionResult> Index()
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var user = _userSession.GetUserSession();
            if (user?.PartyId == null)
            {
                TempData["ErrorMessage"] = "Debe estar asignado a un partido polÃ­tico para gestionar alianzas.";
                return RedirectToRoute(new { controller = "DirigenteHome", action = "Index" });
            }

            var sentRequests = await _allianceService.GetSentRequestsByPartyAsync(user.PartyId.Value);
            var receivedRequests = await _allianceService.GetReceivedRequestsByPartyAsync(user.PartyId.Value);

            var viewModel = new PoliticalAllianceIndexViewModel
            {
                SentRequests = sentRequests,
                ReceivedRequests = receivedRequests,
                CurrentPartyId = user.PartyId.Value
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var user = _userSession.GetUserSession();
            if (user?.PartyId == null)
            {
                TempData["ErrorMessage"] = "Debe estar asignado a un partido polÃ­tico.";
                return RedirectToAction(nameof(Index));
            }

            await LoadAvailableParties(user.PartyId.Value);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SavePoliticalAllianceDTO model)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var user = _userSession.GetUserSession();
            if (user?.PartyId == null)
            {
                TempData["ErrorMessage"] = "Debe estar asignado a un partido polÃ­tico.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await LoadAvailableParties(user.PartyId.Value);
                return View(model);
            }

            if (model.ReceivingPartyId == user.PartyId.Value)
            {
                TempData["ErrorMessage"] = "No puede crear una alianza con su propio partido.";
                await LoadAvailableParties(user.PartyId.Value);
                return View(model);
            }
            var hasActiveAlliance = await _allianceService.HasActiveAllianceAsync(user.PartyId.Value, model.ReceivingPartyId);
            if (hasActiveAlliance)
            {
                TempData["ErrorMessage"] = "Ya existe una solicitud pendiente o alianza activa con este partido.";
                await LoadAvailableParties(user.PartyId.Value);
                return View(model);
            }

            var result = await _allianceService.CreateAllianceRequestAsync(user.PartyId.Value, model.ReceivingPartyId);
            if (result == null)
            {
                TempData["ErrorMessage"] = "Error al crear la solicitud de alianza.";
                await LoadAvailableParties(user.PartyId.Value);
                return View(model);
            }

            TempData["SuccessMessage"] = "Solicitud de alianza enviada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var user = _userSession.GetUserSession();
            if (user?.PartyId == null)
            {
                TempData["ErrorMessage"] = "Debe estar asignado a un partido polÃ­tico.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _allianceService.AcceptAllianceAsync(id, user.PartyId.Value);
            if (!result)
            {
                TempData["ErrorMessage"] = "Error al aceptar la solicitud de alianza.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Solicitud de alianza aceptada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var user = _userSession.GetUserSession();
            if (user?.PartyId == null)
            {
                TempData["ErrorMessage"] = "Debe estar asignado a un partido polÃ­tico.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _allianceService.RejectAllianceAsync(id, user.PartyId.Value);
            if (!result)
            {
                TempData["ErrorMessage"] = "Error al rechazar la solicitud de alianza.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Solicitud de alianza rechazada.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadAvailableParties(int currentPartyId)
        {
            var allParties = await _partyService.GetAll();

            var availableParties = new List<(int Id, string Name, string Acronym)>();

            foreach (var party in allParties)
            {
                if (party.Id == currentPartyId) continue; // Skip own party
                if (!party.IsActive) continue; // Skip inactive parties

                var hasActiveAlliance = await _allianceService.HasActiveAllianceAsync(currentPartyId, party.Id);
                if (!hasActiveAlliance)
                {
                    availableParties.Add((party.Id, party.Name, party.Acronym));
                }
            }

            ViewBag.AvailableParties = availableParties;
        }

        private IActionResult? CheckAuthorization()
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsDirigente())
            {
                TempData["ErrorMessage"] = "No tiene permisos para acceder a esta secciÃ³n.";
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }

            return null;
        }
    }
}
