using eVote360.Application.DTOs.Vote;
using eVote360.Application.Interfaces;
using eVote360.ViewModels.Voting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers
{

    public class VotingController : Controller
    {
        private readonly IOcrService _ocrService;
        private readonly IIdentityValidator _identityValidator;
        private readonly IVotingValidator _votingValidator;
        private readonly IVoteService _voteService;
        private readonly IElectionService _electionService;
        private readonly IElectivePositionService _positionService;
        private readonly ICandidacyService _candidacyService;
        private readonly IEmailService _emailService;
        private readonly ICitizenService _citizenService;
        private readonly IUserSession _userSession;
        private readonly ILogger<VotingController> _logger;

        public VotingController(
            IOcrService ocrService,
            IIdentityValidator identityValidator,
            IVotingValidator votingValidator,
            IVoteService voteService,
            IElectionService electionService,
            IElectivePositionService positionService,
            ICandidacyService candidacyService,
            IEmailService emailService,
            ICitizenService citizenService,
            IUserSession userSession,
            ILogger<VotingController> logger)
        {
            _ocrService = ocrService;
            _identityValidator = identityValidator;
            _votingValidator = votingValidator;
            _voteService = voteService;
            _electionService = electionService;
            _positionService = positionService;
            _candidacyService = candidacyService;
            _emailService = emailService;
            _citizenService = citizenService;
            _userSession = userSession;
            _logger = logger;
        }
        public IActionResult Index()
        {
            // Si un administrador ha iniciado sesion, redirigir al home del administrador
            if (_userSession.IsAdmin())
            {
                return RedirectToAction("Index", "HomeAdmin");
            }

            HttpContext.Session.Remove("VotingData");

            return View(new DocumentInputViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> ValidateDocument(DocumentInputViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }
            var validationResult = await _votingValidator.ValidateCitizenCanVoteAsync(model.DocumentNumber);

            if (!validationResult.IsValid)
            {
                ModelState.AddModelError("", validationResult.Message);
                return View("Index", model);
            }
            HttpContext.Session.SetString("DocumentNumber", model.DocumentNumber);
            HttpContext.Session.SetInt32("CitizenId", validationResult.CitizenId ?? 0);
            HttpContext.Session.SetInt32("ElectionId", validationResult.ElectionId ?? 0);

            return RedirectToAction(nameof(UploadDocument));
        }
        public IActionResult UploadDocument()
        {
            var documentNumber = HttpContext.Session.GetString("DocumentNumber");

            if (string.IsNullOrEmpty(documentNumber))
            {
                return RedirectToAction(nameof(Index));
            }

            return View(new OcrValidationViewModel { DocumentNumber = documentNumber });
        }
        [HttpPost]
        public async Task<IActionResult> ValidateOcr(OcrValidationViewModel model)
        {
            if (model.DocumentImage == null || model.DocumentImage.Length == 0)
            {
                ModelState.AddModelError("DocumentImage", "Debe subir una imagen de su cédula");
                return View("UploadDocument", model);
            }

            var documentNumber = HttpContext.Session.GetString("DocumentNumber");

            if (string.IsNullOrEmpty(documentNumber))
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _logger.LogInformation("Starting OCR validation for document: {DocumentNumber}", documentNumber);
                string extractedText;
                using (var stream = model.DocumentImage.OpenReadStream())
                {
                    _logger.LogInformation("Processing image, size: {Size} bytes", stream.Length);
                    extractedText = await _ocrService.ExtractTextFromImageAsync(stream);
                }

                _logger.LogInformation("OCR extraction completed. Text length: {Length}", extractedText?.Length ?? 0);
                _logger.LogDebug("Extracted text: {Text}", extractedText);

                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    _logger.LogWarning("OCR returned empty text");
                    ModelState.AddModelError("", "No se pudo leer el texto de la imagen. Intente con una imagen más clara.");
                    return View("UploadDocument", model);
                }

                _logger.LogInformation("Validating document {DocumentNumber} against OCR text", documentNumber);
                var isValid = await _identityValidator.ValidateIdentityDocumentAsync(documentNumber, extractedText);

                _logger.LogInformation("Validation result: {IsValid}", isValid);

                if (!isValid)
                {
                    ModelState.AddModelError("", "El número de cédula no coincide con el documento escaneado.");
                    return View("UploadDocument", model);
                }
                _logger.LogInformation("OCR validation successful, redirecting to position selection");
                return RedirectToAction(nameof(SelectPosition));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OCR validation");
                ModelState.AddModelError("", $"Error al procesar la imagen: {ex.Message}");
                return View("UploadDocument", model);
            }
        }
        public async Task<IActionResult> SelectPosition()
        {
            var electionId = HttpContext.Session.GetInt32("ElectionId");
            var citizenId = HttpContext.Session.GetInt32("CitizenId");

            if (!electionId.HasValue || !citizenId.HasValue)
            {
                return RedirectToAction(nameof(Index));
            }
            var election = await _electionService.GetElectionByIdAsync(electionId.Value);

            if (election == null)
            {
                return RedirectToAction(nameof(Index));
            }
            var allCandidacies = await _candidacyService.GetAll();
            var positionIds = allCandidacies.Select(c => c.ElectivePositionId).Distinct().ToList();
            var positions = await _positionService.GetAll();
            var electionPositions = positions.Where(p => positionIds.Contains(p.Id)).ToList();
            var votingData = GetVotingDataFromSession();

            var viewModel = new PositionSelectionViewModel
            {
                ElectionName = election.Name,
                Positions = electionPositions.Select(p => new PositionItemViewModel
                {
                    PositionId = p.Id,
                    PositionName = p.Name,
                    HasVoted = votingData.ContainsKey(p.Id)
                }).ToList()
            };

            return View(viewModel);
        }
        public async Task<IActionResult> SelectCandidate(int positionId)
        {
            var electionId = HttpContext.Session.GetInt32("ElectionId");

            if (!electionId.HasValue)
            {
                return RedirectToAction(nameof(Index));
            }
            var position = await _positionService.GetById(positionId);

            if (position == null)
            {
                return RedirectToAction(nameof(SelectPosition));
            }
            var candidacies = await _candidacyService.GetAll();
            var positionCandidacies = candidacies
                .Where(c => c.ElectivePositionId == positionId)
                .ToList();

            var viewModel = new CandidateSelectionViewModel
            {
                PositionId = positionId,
                PositionName = position.Name,
                Candidates = positionCandidacies.Select(c => new CandidateItemViewModel
                {
                    CandidateId = c.CandidateId,
                    CandidateName = $"{c.CandidateFirstName} {c.CandidateLastName}",
                    PartyAcronym = c.PostulatingPartyAcronym,
                    PhotoPath = null
                }).ToList()
            };

            return View(viewModel);
        }
        [HttpPost]
        public IActionResult Vote(int positionId, int candidateId)
        {
            var electionId = HttpContext.Session.GetInt32("ElectionId");

            if (!electionId.HasValue)
            {
                return RedirectToAction(nameof(Index));
            }
            var votingData = GetVotingDataFromSession();
            votingData[positionId] = candidateId;
            SaveVotingDataToSession(votingData);

            return RedirectToAction(nameof(SelectPosition));
        }
        public async Task<IActionResult> Confirm()
        {
            var electionId = HttpContext.Session.GetInt32("ElectionId");
            var citizenId = HttpContext.Session.GetInt32("CitizenId");

            if (!electionId.HasValue || !citizenId.HasValue)
            {
                return RedirectToAction(nameof(Index));
            }

            var votingData = GetVotingDataFromSession();
            var validationResult = await _votingValidator.ValidateCompleteVotingAsync(
                citizenId.Value,
                electionId.Value,
                votingData);

            if (!validationResult.IsValid)
            {
                TempData["Error"] = validationResult.Message;
                return RedirectToAction(nameof(SelectPosition));
            }
            var election = await _electionService.GetElectionByIdAsync(electionId.Value);
            var citizen = await _citizenService.GetById(citizenId.Value);

            var voteDetails = new List<VoteDetailViewModel>();

            foreach (var vote in votingData)
            {
                var position = await _positionService.GetById(vote.Key);

                if (vote.Value == 0)
                {
                    if (position != null)
                    {
                        voteDetails.Add(new VoteDetailViewModel
                        {
                            PositionName = position.Name,
                            CandidateName = "Ninguno",
                            PartyAcronym = "Voto en Blanco"
                        });
                    }
                }
                else
                {
                    var candidacies = await _candidacyService.GetAll();
                    var candidacy = candidacies.FirstOrDefault(c => c.CandidateId == vote.Value);

                    if (position != null && candidacy != null)
                    {
                        voteDetails.Add(new VoteDetailViewModel
                        {
                            PositionName = position.Name,
                            CandidateName = $"{candidacy.CandidateFirstName} {candidacy.CandidateLastName}",
                            PartyAcronym = candidacy.PostulatingPartyAcronym
                        });
                    }
                }
            }

            var viewModel = new VoteConfirmationViewModel
            {
                ElectionName = election?.Name ?? "",
                CitizenName = $"{citizen?.FirstName} {citizen?.LastName}",
                VoteDetails = voteDetails
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Finalize()
        {
            var electionId = HttpContext.Session.GetInt32("ElectionId");
            var citizenId = HttpContext.Session.GetInt32("CitizenId");

            if (!electionId.HasValue || !citizenId.HasValue)
            {
                return RedirectToAction(nameof(Index));
            }

            var votingData = GetVotingDataFromSession();

            try
            {
                foreach (var vote in votingData)
                {
                    var saveDto = new SaveVoteDTO
                    {
                        ElectionId = electionId.Value,
                        CitizenId = citizenId.Value,
                        CandidateId = vote.Value == 0 ? null : vote.Value,
                        ElectivePositionId = vote.Key
                    };

                    var success = await _voteService.RegisterVoteAsync(saveDto);

                    if (!success)
                    {
                        TempData["Error"] = "Error al registrar el voto. Intente nuevamente.";
                        return RedirectToAction(nameof(Confirm));
                    }
                }
                var citizen = await _citizenService.GetById(citizenId.Value);
                var election = await _electionService.GetElectionByIdAsync(electionId.Value);

                if (citizen != null && !string.IsNullOrEmpty(citizen.Email) && election != null)
                {
                    try
                    {
                        var voteDetailsHtml = new System.Text.StringBuilder();
                        voteDetailsHtml.Append("<!DOCTYPE html>");
                        voteDetailsHtml.Append("<html lang='es'>");
                        voteDetailsHtml.Append("<head><meta charset='UTF-8'><style>");
                        voteDetailsHtml.Append("body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; max-width: 650px; margin: 0 auto; padding: 0; background-color: #f4f4f4; }");
                        voteDetailsHtml.Append(".container { background-color: white; margin: 20px; box-shadow: 0 0 20px rgba(0,0,0,0.1); }");
                        voteDetailsHtml.Append(".header { background: linear-gradient(135deg, #1e7e34 0%, #28a745 100%); color: white; padding: 30px 20px; text-align: center; }");
                        voteDetailsHtml.Append(".header h1 { margin: 0; font-size: 28px; font-weight: 600; letter-spacing: 1px; }");
                        voteDetailsHtml.Append(".header p { margin: 10px 0 0 0; font-size: 14px; opacity: 0.95; }");
                        voteDetailsHtml.Append(".content { padding: 30px; background-color: white; }");
                        voteDetailsHtml.Append(".info-box { background-color: #f8f9fa; padding: 20px; margin: 20px 0; border-left: 4px solid #28a745; border-radius: 4px; }");
                        voteDetailsHtml.Append(".info-box h3 { margin: 0 0 15px 0; color: #1e7e34; font-size: 20px; font-weight: 600; }");
                        voteDetailsHtml.Append(".info-box p { margin: 8px 0; color: #495057; }");
                        voteDetailsHtml.Append(".section-title { color: #1e7e34; font-size: 20px; font-weight: 600; margin: 25px 0 15px 0; padding-bottom: 10px; border-bottom: 2px solid #28a745; }");
                        voteDetailsHtml.Append("table { width: 100%; border-collapse: collapse; margin: 15px 0; background-color: white; border: 1px solid #dee2e6; }");
                        voteDetailsHtml.Append("th { background-color: #343a40; color: white; padding: 14px 12px; text-align: left; font-weight: 600; font-size: 14px; text-transform: uppercase; letter-spacing: 0.5px; }");
                        voteDetailsHtml.Append("td { padding: 12px; border-bottom: 1px solid #dee2e6; color: #495057; }");
                        voteDetailsHtml.Append("tr:last-child td { border-bottom: none; }");
                        voteDetailsHtml.Append("tr:nth-child(even) { background-color: #f8f9fa; }");
                        voteDetailsHtml.Append(".badge { display: inline-block; padding: 6px 12px; background-color: #007bff; color: white; border-radius: 4px; font-size: 11px; font-weight: 600; letter-spacing: 0.5px; }");
                        voteDetailsHtml.Append(".warning-box { background-color: #fff3cd; padding: 20px; margin: 20px 0; border-left: 4px solid #ffc107; border-radius: 4px; }");
                        voteDetailsHtml.Append(".warning-title { color: #856404; font-weight: 700; font-size: 16px; margin: 0 0 12px 0; }");
                        voteDetailsHtml.Append(".warning-box ul { margin: 10px 0; padding-left: 20px; color: #856404; }");
                        voteDetailsHtml.Append(".warning-box li { margin: 8px 0; }");
                        voteDetailsHtml.Append(".footer { background-color: #343a40; color: #adb5bd; padding: 25px; text-align: center; font-size: 12px; }");
                        voteDetailsHtml.Append(".footer p { margin: 5px 0; }");
                        voteDetailsHtml.Append(".footer-divider { border-top: 1px solid #495057; margin: 15px 0; }");
                        voteDetailsHtml.Append("</style></head>");
                        voteDetailsHtml.Append("<body>");
                        voteDetailsHtml.Append("<div class='container'>");
                        voteDetailsHtml.Append("<div class='header'>");
                        voteDetailsHtml.Append("<h1>CONFIRMACIÓN DE VOTACIÓN</h1>");
                        voteDetailsHtml.Append("<p>Sistema de Votación Electrónica eVote360</p>");
                        voteDetailsHtml.Append("</div>");
                        voteDetailsHtml.Append("<div class='content'>");
                        voteDetailsHtml.Append($"<p style='font-size: 16px;'>Estimado/a <strong>{citizen.FirstName} {citizen.LastName}</strong>,</p>");
                        voteDetailsHtml.Append("<p>Su voto ha sido registrado exitosamente en el sistema. A continuación encontrará el resumen detallado de su participación electoral:</p>");
                        voteDetailsHtml.Append("<div class='info-box'>");
                        voteDetailsHtml.Append($"<h3>{election.Name}</h3>");
                        voteDetailsHtml.Append($"<p><strong>Fecha de votación:</strong> {DateTime.Now:dd/MM/yyyy}</p>");
                        voteDetailsHtml.Append($"<p><strong>Hora de registro:</strong> {DateTime.Now:HH:mm:ss}</p>");
                        voteDetailsHtml.Append($"<p><strong>Documento de identidad:</strong> {citizen.IdentityDocument}</p>");
                        voteDetailsHtml.Append("</div>");
                        voteDetailsHtml.Append("<h3 class='section-title'>RESUMEN DE VOTOS EMITIDOS</h3>");
                        voteDetailsHtml.Append("<table>");
                        voteDetailsHtml.Append("<thead><tr><th>Puesto Electivo</th><th>Candidato Seleccionado</th><th>Partido Político</th></tr></thead>");
                        voteDetailsHtml.Append("<tbody>");

                        foreach (var vote in votingData)
                        {
                            var position = await _positionService.GetById(vote.Key);

                            if (position != null)
                            {
                                voteDetailsHtml.Append("<tr>");
                                voteDetailsHtml.Append($"<td><strong>{position.Name}</strong></td>");

                                if (vote.Value == 0)
                                {
                                    voteDetailsHtml.Append("<td><em>Ninguno</em></td>");
                                    voteDetailsHtml.Append("<td><span class='badge' style='background-color: #6c757d;'>Voto en Blanco</span></td>");
                                }
                                else
                                {
                                    var candidacies = await _candidacyService.GetAll();
                                    var candidacy = candidacies.FirstOrDefault(c => c.CandidateId == vote.Value);

                                    if (candidacy != null)
                                    {
                                        voteDetailsHtml.Append($"<td>{candidacy.CandidateFirstName} {candidacy.CandidateLastName}</td>");
                                        voteDetailsHtml.Append($"<td><span class='badge'>{candidacy.PostulatingPartyAcronym}</span></td>");
                                    }
                                }

                                voteDetailsHtml.Append("</tr>");
                            }
                        }

                        voteDetailsHtml.Append("</tbody></table>");
                        voteDetailsHtml.Append("<div class='warning-box'>");
                        voteDetailsHtml.Append("<p class='warning-title'>INFORMACIÓN IMPORTANTE DE SEGURIDAD Y PRIVACIDAD</p>");
                        voteDetailsHtml.Append("<ul>");
                        voteDetailsHtml.Append("<li>Su voto ha sido registrado de manera segura, secreta y anónima</li>");
                        voteDetailsHtml.Append("<li>Esta confirmación es únicamente para sus registros personales</li>");
                        voteDetailsHtml.Append("<li>No comparta esta información con terceros</li>");
                        voteDetailsHtml.Append("<li>Los resultados oficiales serán publicados una vez finalice el proceso electoral</li>");
                        voteDetailsHtml.Append("<li>Conserve este correo como comprobante de su participación electoral</li>");
                        voteDetailsHtml.Append("</ul>");
                        voteDetailsHtml.Append("</div>");

                        voteDetailsHtml.Append("<p style='margin-top: 25px; font-size: 15px; color: #495057;'>Agradecemos su participación en este proceso democrático. Su voto es fundamental para el fortalecimiento de nuestras instituciones.</p>");
                        voteDetailsHtml.Append("<p style='margin-top: 15px; font-size: 14px; color: #6c757d;'>Atentamente,<br><strong>Junta Central Electoral</strong><br>Sistema eVote360</p>");
                        voteDetailsHtml.Append("</div>");
                        voteDetailsHtml.Append("<div class='footer'>");
                        voteDetailsHtml.Append("<p><strong>eVote360 - Sistema de Votación Electrónica</strong></p>");
                        voteDetailsHtml.Append("<div class='footer-divider'></div>");
                        voteDetailsHtml.Append("<p>Este es un mensaje automático generado por el sistema. Por favor no responda a este correo.</p>");
                        voteDetailsHtml.Append($"<p>Copyright © {DateTime.Now.Year} eVote360. Todos los derechos reservados.</p>");
                        voteDetailsHtml.Append("</div>");

                        voteDetailsHtml.Append("</div></body></html>");
                        await _emailService.SendAsync(new eVote360.Application.DTOs.Email.EmailRequestDto
                        {
                            To = citizen.Email,
                            Subject = $"Confirmación de Votación - {election.Name}",
                            HtmlBody = voteDetailsHtml.ToString()
                        });

                        _logger.LogInformation("Confirmation email sent to {Email} for election {ElectionId}",
                            citizen.Email, electionId.Value);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Error sending confirmation email to {Email}", citizen.Email);
                    }
                }
                HttpContext.Session.Remove("VotingData");
                HttpContext.Session.Remove("DocumentNumber");
                HttpContext.Session.Remove("CitizenId");
                HttpContext.Session.Remove("ElectionId");

                TempData["Success"] = "¡Su voto ha sido registrado exitosamente!";
                return RedirectToAction(nameof(Complete));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al finalizar la votación: {ex.Message}";
                return RedirectToAction(nameof(Confirm));
            }
        }
        public IActionResult Complete()
        {
            return View();
        }

        #region Helper Methods

        private Dictionary<int, int> GetVotingDataFromSession()
        {
            var json = HttpContext.Session.GetString("VotingData");

            if (string.IsNullOrEmpty(json))
            {
                return new Dictionary<int, int>();
            }

            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(json)
                   ?? new Dictionary<int, int>();
        }

        private void SaveVotingDataToSession(Dictionary<int, int> votingData)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(votingData);
            HttpContext.Session.SetString("VotingData", json);
        }

        #endregion
    }
}
