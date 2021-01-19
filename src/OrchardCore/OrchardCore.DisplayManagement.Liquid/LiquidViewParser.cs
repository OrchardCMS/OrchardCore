using Fluid;
using OrchardCore.DisplayManagement.Liquid.Tags;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidViewParser : FluidParser
    {
        public LiquidViewParser()
        {
            RegisterEmptyTag("render_body", (writer, encoder, context)
                => RenderBodyTag.WriteToAsync(writer, encoder, context));
            //RegisterIdentifierTag("render_section", (s, w, e, c) => RenderSectionTag.WriteToAsync(w, e, c, null));
            //RegisterIdentifierTag("page_title", (s, w, e, c) => RenderTitleSegmentsTag.WriteToAsync(w, e, c, null));
            RegisterEmptyTag("antiforgerytoken", (writer, encoder, context)
                => AntiForgeryTokenTag.WriteToAsync(writer, encoder, context));

            RegisterEmptyTag("antiforgerytoken", (writer, encoder, context)
                => AntiForgeryTokenTag.WriteToAsync(writer, encoder, context));

            RegisterExpressionBlock("layout", (expression, statements, writer, encoder, context)
                => LayoutTag.WriteToAsync(writer, encoder, context, expression));
        }
    }
}
