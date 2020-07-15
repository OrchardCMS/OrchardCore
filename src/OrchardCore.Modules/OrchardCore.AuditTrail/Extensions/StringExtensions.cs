using System.Text.RegularExpressions;

namespace OrchardCore.AuditTrail.Extensions
{
    public static class StringExtensions
    {
        public static string NewlinesToHtml(this string value) =>
            string.IsNullOrWhiteSpace(value) ? value : Regex.Replace(value, @"\n", "<br/>");
    }
}
