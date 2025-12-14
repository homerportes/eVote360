namespace eVote360.Application.Interfaces
{
    /// <summary>
    /// Interface for OCR (Optical Character Recognition) operations
    /// Single Responsibility: Only handles OCR text extraction
    /// </summary>
    public interface IOcrService
    {
        Task<string> ExtractTextFromImageAsync(string imagePath);
        Task<string> ExtractTextFromImageAsync(Stream imageStream);
    }
}
