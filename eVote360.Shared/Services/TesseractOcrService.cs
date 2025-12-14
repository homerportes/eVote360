using eVote360.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Tesseract;

namespace eVote360.Shared.Services
{
    /// <summary>
    /// OCR Service implementation using Tesseract
    /// Single Responsibility Principle: Only handles OCR operations
    /// </summary>
    public class TesseractOcrService : IOcrService
    {
        private readonly ILogger<TesseractOcrService> _logger;
        private readonly string _tessDataPath;

        public TesseractOcrService(ILogger<TesseractOcrService> logger)
        {
            _logger = logger;
            // Path to tessdata folder (needs to be configured)
            _tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

            if (!Directory.Exists(_tessDataPath))
            {
                _logger.LogWarning("Tessdata directory not found at {Path}. OCR may not work properly.", _tessDataPath);
            }
        }

        public async Task<string> ExtractTextFromImageAsync(string imagePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!File.Exists(imagePath))
                    {
                        _logger.LogError("Image file not found at {Path}", imagePath);
                        return string.Empty;
                    }

                    using var engine = new TesseractEngine(_tessDataPath, "spa+eng", EngineMode.Default);
                    using var img = Pix.LoadFromFile(imagePath);
                    using var page = engine.Process(img);

                    var text = page.GetText();
                    _logger.LogInformation("OCR extracted {Length} characters from image", text.Length);

                    return text;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error extracting text from image: {Message}", ex.Message);
                    return string.Empty;
                }
            });
        }

        public async Task<string> ExtractTextFromImageAsync(Stream imageStream)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var engine = new TesseractEngine(_tessDataPath, "spa+eng", EngineMode.Default);

                    // Save stream to temporary file
                    var tempPath = Path.Combine(Path.GetTempPath(), $"ocr_{Guid.NewGuid()}.jpg");

                    using (var fileStream = File.Create(tempPath))
                    {
                        imageStream.CopyTo(fileStream);
                    }

                    using var img = Pix.LoadFromFile(tempPath);
                    using var page = engine.Process(img);

                    var text = page.GetText();

                    // Clean up temp file
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }

                    _logger.LogInformation("OCR extracted {Length} characters from stream", text.Length);

                    return text;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error extracting text from stream: {Message}", ex.Message);
                    return string.Empty;
                }
            });
        }
    }
}
