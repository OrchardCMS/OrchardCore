namespace OrchardCore.DisplayManagement.Utilities;

public static class StringExtensions
{
    /// <summary>
    /// Encodes dashed and dots so that they don't conflict in filenames.
    /// </summary>
    /// <param name="alternateElement"></param>
    /// <returns></returns>
    public static string EncodeAlternateElement(this string alternateElement)
    {
        return alternateElement.Replace("-", "__").Replace('.', '_');
    }
}
