using System.Collections.Generic;

namespace OrchardCore.Media
{
    /// <summary>
    /// Provides an extension methods for <see cref="IOrchardHelper"/>.
    /// </summary>
    public static class OrchardHelperExtensions
    {
        private static readonly HashSet<string> _imageExtensions = new HashSet<string>() { ".bmp", ".gif", ".jpeg", ".jpg", ".png", ".tiff", ".webp" };

        /// <summary>
        /// Determines if an extension is for an image file.
        /// </summary>
        /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
        /// <param name="extension">The extension.</param>
        /// <returns>true if the extension is for an image file. Otherwise false.</returns>
        public static bool IsImageExtension(this IOrchardHelper orchardHelper, string extension)
        {
            return _imageExtensions.Contains(extension.ToLower());
        }
    }
}
