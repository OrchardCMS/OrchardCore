using System.Text.Json;

namespace OrchardCore;

public class JsonHelpers
{
    public static bool IsValid(string json, JsonDocumentOptions options = default)
    {
        try
        {
            JsonDocument.Parse(json, options);

            return true;
        }
        catch { }

        return false;
    }

    public static bool TryParse(string json, out JsonDocument document, JsonDocumentOptions options = default)
    {
        try
        {
            document = JsonDocument.Parse(json, options);

            return true;
        }
        catch { }

        document = null;
        return false;
    }
}
