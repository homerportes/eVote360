using eVote360.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace eVote360.Application.Services
{
    /// <summary>
    /// Identity Validator Service
    /// Single Responsibility Principle: Only validates identity documents
    /// </summary>
    public class IdentityValidatorService : IIdentityValidator
    {
        private readonly ILogger<IdentityValidatorService> _logger;

        public IdentityValidatorService(ILogger<IdentityValidatorService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> ValidateIdentityDocumentAsync(string documentNumber, string ocrExtractedText)
        {
            return await Task.Run(() =>
            {
                _logger.LogInformation("Validating document: {DocumentNumber}", documentNumber);

                if (string.IsNullOrWhiteSpace(documentNumber) || string.IsNullOrWhiteSpace(ocrExtractedText))
                {
                    _logger.LogWarning("Document number or OCR text is empty");
                    return false;
                }

                // Extract document number from OCR text
                var extractedNumber = ExtractDocumentNumberFromText(ocrExtractedText);

                _logger.LogInformation("Extracted number from OCR: {ExtractedNumber}", extractedNumber);

                if (string.IsNullOrWhiteSpace(extractedNumber))
                {
                    _logger.LogWarning("Could not extract document number from OCR text");
                    return false;
                }

                // Compare extracted number with provided document number
                // Remove spaces, dashes and special characters for comparison
                var cleanDocumentNumber = CleanDocumentNumber(documentNumber);
                var cleanExtractedNumber = CleanDocumentNumber(extractedNumber);

                _logger.LogInformation("Comparing: Clean Input={CleanInput}, Clean Extracted={CleanExtracted}",
                    cleanDocumentNumber, cleanExtractedNumber);

                var isValid = cleanDocumentNumber.Equals(cleanExtractedNumber, StringComparison.OrdinalIgnoreCase);

                _logger.LogInformation("Validation result: {IsValid}", isValid);

                return isValid;
            });
        }

        public string ExtractDocumentNumberFromText(string ocrText)
        {
            if (string.IsNullOrWhiteSpace(ocrText))
            {
                return string.Empty;
            }

            _logger.LogDebug("Attempting to extract document number from text: {Text}", ocrText);

            // Common patterns for identity document numbers
            // Pattern 1: 000-0000000-0 (Dominican ID format)
            var pattern1 = @"\d{3}-?\d{7}-?\d{1}";

            // Pattern 2: Sequence of 11 digits
            var pattern2 = @"\d{11}";

            // Pattern 3: Look for "No." or "Cedula" followed by numbers
            var pattern3 = @"(?:No\.|Cedula|Cédula|ID)[:\s]*(\d{3}-?\d{7}-?\d{1}|\d{11})";

            // Try pattern 3 first (with context)
            var match3 = Regex.Match(ocrText, pattern3, RegexOptions.IgnoreCase);
            if (match3.Success && match3.Groups.Count > 1)
            {
                _logger.LogInformation("Match found with Pattern 3 (contextual): {Match}", match3.Groups[1].Value);
                return match3.Groups[1].Value;
            }

            // Try pattern 1 (formatted)
            var match1 = Regex.Match(ocrText, pattern1);
            if (match1.Success)
            {
                _logger.LogInformation("Match found with Pattern 1 (formatted): {Match}", match1.Value);
                return match1.Value;
            }

            // Try pattern 2 (plain digits)
            var match2 = Regex.Match(ocrText, pattern2);
            if (match2.Success)
            {
                _logger.LogInformation("Match found with Pattern 2 (11 digits): {Match}", match2.Value);
                return match2.Value;
            }

            _logger.LogWarning("No document number pattern matched in OCR text");
            return string.Empty;
        }

        private string CleanDocumentNumber(string documentNumber)
        {
            // Remove all non-alphanumeric characters
            return Regex.Replace(documentNumber, @"[^a-zA-Z0-9]", "");
        }
    }
}
