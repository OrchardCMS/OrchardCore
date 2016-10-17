using Microsoft.Extensions.Localization;

namespace Orchard.Localization
{
    /// <summary>
    /// Localizes some text based on the current Work Context culture
    /// </summary>
    /// <param name="text">The text format to localize</param>
    /// <param name="args">The arguments used in the text format. The arguments are HTML-encoded if they don't implement <see cref="System.Web.IHtmlString"/>.</param>
    /// <returns>An HTML-encoded localized string</returns>
    public delegate LocalizedString Localizer(string text, params object[] args);
}