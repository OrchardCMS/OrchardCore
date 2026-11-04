using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Tags;

public class AntiForgeryTokenTag
{
    public static ValueTask<Completion> WriteToAsync(IFluidOutput output, TextEncoder encoder, TemplateContext context)
    {
        var services = ((LiquidTemplateContext)context).Services;

        var antiforgery = services.GetRequiredService<IAntiforgery>();
        var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();

        var httpContext = httpContextAccessor.HttpContext;
        var tokenSet = antiforgery.GetAndStoreTokens(httpContext);

        output.Write("<input name=\"");
        output.Write(encoder.Encode(tokenSet.FormFieldName));
        output.Write("\" type=\"hidden\" value=\"");
        output.Write(encoder.Encode(tokenSet.RequestToken));
        output.Write("\" />");

        return ValueTask.FromResult(Completion.Normal);
    }
}
