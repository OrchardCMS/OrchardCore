using System;
using System.Collections.Generic;
using OrchardCore;

/// <summary>
/// Provides an extension methods for <see cref="IOrchardHelper"/>.
/// </summary>
#pragma warning disable CA1050 // Declare types in namespaces
public static class MediaOrchardHelperExtensions
#pragma warning restore CA1050 // Declare types in namespaces
{
    private static readonly HashSet<string> _imageExtensions = new(StringComparer.OrdinalIgnoreCase) { ".bmp", ".gif", ".jpeg", ".jpg", ".png", ".webp" };

    /// <summary>
    /// Determines if a path is an image file.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="path">The path.</param>
    /// <returns>true if the path is an image file. Otherwise false.</returns>
#pragma warning disable IDE0060 // Remove unused parameter
    public static bool IsImageFile(this IOrchardHelper orchardHelper, string path)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        return _imageExtensions.Contains(System.IO.Path.GetExtension(path));
    }
}
