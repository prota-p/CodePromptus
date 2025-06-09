using System.Text;

namespace CodePromptus.App.Infrastructure;

public interface ITextEncodingDetectionService
{
    Encoding DetectEncoding(string filePath);
}
