using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class SlotTag : ArgumentsTag
    {
        public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, FilterArgument[] arguments)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'slot'");
            }

            var shapeScopeManager = ((IServiceProvider)services).GetRequiredService<IShapeScopeManager>();

            var slot = (await arguments[0].Expression.EvaluateAsync(context)).ToStringValue();

            if (!String.IsNullOrEmpty(slot))
            {
                var content = shapeScopeManager.GetSlot(slot);
                if (content != null)
                {
                    if (content is string stringContent)
                    {
                        await writer.WriteAsync(content);
                    } else if (content is IHtmlContent htmlContent)
                    {
                        // TODO Reevaluate how we handle this.
                        // I think it will be either a string from a liquid block, or htmlcontent from a taghelper
                        // but can it be a shape as well ?
                        // var value = new HtmlContentValue(htmlString);
                        // value.WriteTo(writer, encoder, null);

                        // Hmm this assume we always have an html encoder to work with.

                        htmlContent.WriteTo(writer, (HtmlEncoder)encoder);
                    }
                }
            }

            return Completion.Normal;
        }
    }
}
