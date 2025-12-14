namespace eVote360.Application.Interfaces
{
    /// <summary>
    /// Interface for identity document validation
    /// Single Responsibility: Validates identity documents
    /// </summary>
    public interface IIdentityValidator
    {
        Task<bool> ValidateIdentityDocumentAsync(string documentNumber, string ocrExtractedText);
        string ExtractDocumentNumberFromText(string ocrText);
    }
}
