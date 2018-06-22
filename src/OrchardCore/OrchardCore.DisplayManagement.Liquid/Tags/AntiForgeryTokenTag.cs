using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class AntiForgeryTokenTag : SimpleTag
    {
        public Expression Shape { get; }

        public override Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'antiforgerytoken'");
            }

            var antiforgery = ((IServiceProvider)services).GetRequiredService<IAntiforgery>();
            var httpContextAccessor = ((IServiceProvider)services).GetRequiredService<IHttpContextAccessor>();

            var httpContext = httpContextAccessor.HttpContext;
            var tokenSet = antiforgery.GetAndStoreTokens(httpContext);

            writer.Write("<input name=\"");
            HtmlEncoder.Default.Encode(writer, tokenSet.FormFieldName);
            writer.Write("\" type=\"hidden\" value=\"");
            HtmlEncoder.Default.Encode(writer, tokenSet.RequestToken);
            writer.Write("\" />");

            return Task.FromResult(Completion.Normal);
        }
    }
}