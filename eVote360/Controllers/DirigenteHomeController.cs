using eVote360.Application.DTOs.Common;
using eVote360.Application.Interfaces;
using eVote360.Application.ViewModels.DirigenteHome;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    public class DirigenteHomeController : Controller
    {
        private readonly IUserSession _userSession;
        private readonly IPoliticalPartyService _partyService;
        private readonly ICandidateService _candidateService;
        private readonly IPoliticalAllianceService _allianceService;
        private readonly ICandidacyService _candidacyService;

        public DirigenteHomeController(
            IUserSession userSession,
            IPoliticalPartyService partyService,
            ICandidateService candidateService,
            IPoliticalAllianceService allianceService,
            ICandidacyService candidacyService)
        {
            _userSession = userSession;
            _partyService = partyService;
            _candidateService = candidateService;
            _allianceService = allianceService;
            _candidacyService = candidacyService;
        }

        public async Task<IActionResult> Index()
        {
            if (!_userSession.HasUser())
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            if (!_userSession.IsDirigente())
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });

            var user = _userSession.GetUserSession();
            if (user == null || !user.PartyId.HasValue)
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            var partyId = user.PartyId.Value;
            var viewModel = new DirigenteHomeIndexViewModel();

            viewModel.Party = await _partyService.GetById(partyId);

            var indicators = new DashboardIndicatorsDTO
            {
                ActiveCandidates = await _candidateService.CountActiveCandidatesByPartyAsync(partyId),
                InactiveCandidates = await _candidateService.CountInactiveCandidatesByPartyAsync(partyId),
                TotalAlliances = await _allianceService.CountAcceptedAlliancesByPartyAsync(partyId),
                PendingAllianceRequests = await _allianceService.CountPendingRequestsByPartyAsync(partyId),
                CandidatesAssignedToPositions = await _candidacyService.CountCandidatesAssignedToPositionsByPartyAsync(partyId)
            };

            viewModel.Indicators = indicators;

            return View(viewModel);
        }
    }
}
