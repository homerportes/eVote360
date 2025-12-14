using eVote360.Application.DTOs.Election;
using eVote360.Application.Interfaces;
using eVote360.Application.ViewModels.Election;
using eVote360.Domain.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{
    public class ElectionController : Controller
    {
        private readonly IElectionService _electionService;
        private readonly IUserSession _userSession;

        public ElectionController(
            IElectionService electionService,
            IUserSession userSession)
        {
            _electionService = electionService;
            _userSession = userSession;
        }

        public async Task<IActionResult> Index()
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var elections = await _electionService.GetAllElectionsAsync();
            var hasActiveElection = elections.Any(e => e.Status == ElectionStatus.InProcess);

            var viewModel = new ElectionIndexViewModel
            {
                Elections = elections,
                HasActiveElection = hasActiveElection
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;
            var hasActiveElection = await _electionService.HasActiveElectionAsync();
            if (hasActiveElection)
            {
                TempData["ErrorMessage"] = "Ya existe una elecciÃ³n activa. Debe finalizarla antes de crear una nueva.";
                return RedirectToAction(nameof(Index));
            }
            var validation = await _electionService.ValidateElectionCreationAsync();
            ViewBag.ValidationResult = validation;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveElectionDTO model)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            if (!ModelState.IsValid)
            {
                var validation = await _electionService.ValidateElectionCreationAsync();
                ViewBag.ValidationResult = validation;
                return View(model);
            }

            var validationResult = await _electionService.ValidateElectionCreationAsync();
            if (!validationResult.IsValid)
            {
                ViewBag.ValidationResult = validationResult;
                return View(model);
            }

            var result = await _electionService.CreateElectionAsync(model);
            if (result == null)
            {
                TempData["ErrorMessage"] = "Error al crear la elecciÃ³n.";
                ViewBag.ValidationResult = validationResult;
                return View(model);
            }

            TempData["SuccessMessage"] = "ElecciÃ³n creada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Finish(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var elections = await _electionService.GetAllElectionsAsync();
            var election = elections.FirstOrDefault(e => e.Id == id);

            if (election == null)
            {
                TempData["ErrorMessage"] = "ElecciÃ³n no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            if (election.Status != ElectionStatus.InProcess)
            {
                TempData["ErrorMessage"] = "Solo se pueden finalizar elecciones activas.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new FinishElectionViewModel
            {
                Id = election.Id,
                Name = election.Name,
                Date = election.Date
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinishConfirmed(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var result = await _electionService.FinishElectionAsync(id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Error al finalizar la elecciÃ³n.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "ElecciÃ³n finalizada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Results(int id)
        {
            var authCheck = CheckAuthorization();
            if (authCheck != null) return authCheck;

            var results = await _electionService.GetElectionResultsAsync(id);
            if (results == null)
            {
                TempData["ErrorMessage"] = "No se pueden ver los resultados de esta elecciÃ³n.";
                return RedirectToAction(nameof(Index));
            }

            return View(results);
        }

        private IActionResult? CheckAuthorization()
        {
            if (!_userSession.HasUser())
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!_userSession.IsAdmin())
            {
                TempData["ErrorMessage"] = "No tiene permisos para acceder a esta secciÃ³n.";
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }

            return null;
        }
    }
}
