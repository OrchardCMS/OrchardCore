using System.Text.Json.Nodes;

namespace OrchardCore.Media.Fields;

public static class MediaFieldAttachedFileNameExtensions
{
    /// <summary>
    /// Gets the names of <see cref="MediaField"/> attached files.
    /// </summary>
    public static string[] GetAttachedFileNames(this MediaField mediaField)
    {
        var filenames = (JsonArray)mediaField.Content["AttachedFileNames"];

        return filenames != null
            ? filenames.ToObject<string[]>()
            : [];
    }

    /// <summary>
    /// Sets the names of <see cref="MediaField"/> attached files.
    /// </summary>
    public static void SetAttachedFileNames(this MediaField mediaField, string[] filenames)
    {
        mediaField.Content["AttachedFileNames"] = JArray.FromObject(filenames);
    }

}
