using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class AntiForgeryTokenTag
    {
        public static ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var services = ((LiquidTemplateContext)context).Services;
            
            var antiforgery = services.GetRequiredService<IAntiforgery>();
            var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();

            var httpContext = httpContextAccessor.HttpContext;
            var tokenSet = antiforgery.GetAndStoreTokens(httpContext);

            writer.Write("<input name=\"");
            encoder.Encode(writer, tokenSet.FormFieldName);
            writer.Write("\" type=\"hidden\" value=\"");
            encoder.Encode(writer, tokenSet.RequestToken);
            writer.Write("\" />");

            return new ValueTask<Completion>(Completion.Normal);
        }
    }
}
