namespace OrchardCore.AuditTrail.Extensions
{
    public static class StringExtensions
    {
        public static string NewlinesToHtml(this string value) => value?.Replace("\r", "")?.Replace("\n", "<br />");
    }
}
