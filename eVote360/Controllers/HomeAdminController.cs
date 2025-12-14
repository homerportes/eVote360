using eVote360.Application.Interfaces;
using eVote360.Application.ViewModels.HomeAdmin;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    public class HomeAdminController : Controller
    {
        private readonly IUserSession _userSession;
        private readonly IElectionService _electionService;

        public HomeAdminController(IUserSession userSession, IElectionService electionService)
        {
            _userSession = userSession;
            _electionService = electionService;
        }

        public async Task<IActionResult> Index(int? year)
        {
            if (!_userSession.HasUser())
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            if (!_userSession.IsAdmin())
                return RedirectToRoute(new { controller = "Login", action = "AccessDenied" });

            var viewModel = new HomeAdminIndexViewModel();

            var availableYears = await _electionService.GetAvailableElectionYearsAsync();
            viewModel.AvailableYears = availableYears;

            if (year.HasValue)
            {
                viewModel.SelectedYear = year.Value;
                viewModel.ElectionSummaries = await _electionService.GetElectionSummariesByYearAsync(year.Value);
            }
            else if (availableYears.Any())
            {
                viewModel.SelectedYear = availableYears.First(); // Most recent year
                viewModel.ElectionSummaries = await _electionService.GetElectionSummariesByYearAsync(availableYears.First());
            }

            return View(viewModel);
        }
    }
}
