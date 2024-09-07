namespace OrchardCore.Search.AzureAI;

public static class AzureAISearchIndexNamingHelper
{
    /// <summary>
    /// Makes sure that the index names are compliant with Azure AI Search specifications.
    /// <see href="https://learn.microsoft.com/en-us/rest/api/searchservice/naming-rules"/>.
    /// </summary>
    public static bool TryGetSafeIndexName(string indexName, out string safeName)
    {
        if (!TryGetSafePrefix(indexName, out var safePrefix) || safePrefix.Length < 2)
        {
            safeName = null;

            return false;
        }

        if (safePrefix.Length > 128)
        {
            safeName = safePrefix[..128];
        }
        else
        {
            safeName = safePrefix;
        }

        return true;
    }

    public static bool TryGetSafeFieldName(string fieldName, out string safeName)
    {
        if (string.IsNullOrEmpty(fieldName))
        {
            safeName = null;

            return false;
        }

        if (fieldName.StartsWith("azureSearch"))
        {
            fieldName = fieldName[11..];
        }

        while (fieldName.Length > 0 && !char.IsLetter(fieldName[0]))
        {
            fieldName = fieldName.Remove(0, 1);
        }

        fieldName = fieldName.Replace(".", "__");

        var validChars = Array.FindAll(fieldName.ToCharArray(), c => char.IsLetterOrDigit(c) || c == '_');

        if (validChars.Length > 128)
        {
            safeName = new string(validChars[..128]);

            return true;
        }

        if (validChars.Length > 0)
        {
            safeName = new string(validChars);

            return true;
        }

        safeName = null;

        return false;
    }

    public static bool TryGetSafePrefix(string indexName, out string safePrefix)
    {
        if (string.IsNullOrWhiteSpace(indexName))
        {
            safePrefix = null;

            return false;
        }

        indexName = indexName.ToLowerInvariant();

        while (indexName.Length > 0 && !char.IsLetterOrDigit(indexName[0]))
        {
            indexName = indexName.Remove(0, 1);
        }

        var validChars = Array.FindAll(indexName.ToCharArray(), c => char.IsLetterOrDigit(c) || c == '-');

        safePrefix = new string(validChars);

        while (safePrefix.Contains("--"))
        {
            safePrefix = safePrefix.Replace("--", "-");
        }

        return true;
    }
}
