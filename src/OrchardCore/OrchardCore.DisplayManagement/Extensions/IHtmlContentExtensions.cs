using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;


namespace OrchardCore.DisplayManagement.Extensions;

public static class IHtmlContentExtensions
{
  public static string GetString(this IHtmlContent content)
  {
    using var writer = new System.IO.StringWriter();
    content.WriteTo(writer, HtmlEncoder.Default);
    return writer.ToString();
  }
}