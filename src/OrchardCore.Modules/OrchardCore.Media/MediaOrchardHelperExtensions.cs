using System;
using System.Collections.Generic;
using OrchardCore;

/// <summary>
/// Provides an extension methods for <see cref="IOrchardHelper"/>.
/// </summary>
public static class MediaOrchardHelperExtensions
{
    private static readonly HashSet<string> _imageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".bmp", ".gif", ".jpeg", ".jpg", ".png", ".tiff", ".webp" };

    /// <summary>
    /// Determines if a path is an image file.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="path">The path.</param>
    /// <returns>true if the path is an image file. Otherwise false.</returns>
    public static bool IsImageFile(this IOrchardHelper orchardHelper, string path)
    {
        return _imageExtensions.Contains(System.IO.Path.GetExtension(path));
    }
}
