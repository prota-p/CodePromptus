using System.Text;
using UtfUnknown;

namespace CodePromptus.App.Infrastructure;

public class UtfUnknownTextEncodingDetectionService : ITextEncodingDetectionService
{
    public Encoding DetectEncoding(string filePath)
    {
        var detectionResult = CharsetDetector.DetectFromFile(filePath);
        return detectionResult.Detected?.Encoding ?? Encoding.UTF8;
    }
}
