using Fluid;
using OrchardCore.DisplayManagement.Liquid.Tags;
using OrchardCore.DynamicCache.Liquid;
using Parlot.Fluent;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidViewParser : FluidParser
    {
        static LiquidViewParser()
        {
            FluidTagHelper.DefaultArgumentsMapping["shape"] = "type";
            FluidTagHelper.DefaultArgumentsMapping["zone"] = "name";
        }

        public LiquidViewParser()
        {
            RegisterEmptyTag("render_body", RenderBodyTag.WriteToAsync);
            RegisterParserTag("render_section", ArgumentsList, RenderSectionTag.WriteToAsync);
            RegisterParserTag("page_title", ArgumentsList, RenderTitleSegmentsTag.WriteToAsync);
            RegisterEmptyTag("antiforgerytoken", AntiForgeryTokenTag.WriteToAsync);
            RegisterExpressionTag("layout", LayoutTag.WriteToAsync);

            RegisterExpressionTag("shape_clear_alternates", ClearAlternatesTag.WriteToAsync);
            RegisterParserTag("shape_add_alternates", Primary.And(Primary), AddAlternatesTag.WriteToAsync);
            RegisterExpressionTag("shape_clear_wrappers", ClearWrappers.WriteToAsync);
            RegisterParserTag("shape_add_wrappers", Primary.And(Primary), AddWrappersTag.WriteToAsync);
            RegisterExpressionTag("shape_clear_classes", ClearClassesTag.WriteToAsync);
            RegisterParserTag("shape_add_classes", Primary.And(Primary), AddClassesTag.WriteToAsync);
            RegisterExpressionTag("shape_clear_attributes", ClearAttributesTag.WriteToAsync);
            RegisterParserTag("shape_add_attributes", Primary.And(ArgumentsList), AddAttributesTag.WriteToAsync);
            RegisterParserTag("shape_type", Primary.And(Primary), ShapeTypeTag.WriteToAsync);
            RegisterParserTag("shape_display_type", Primary.And(Primary), ShapeDisplayTypeTag.WriteToAsync);
            RegisterParserTag("shape_position", Primary.And(Primary), ShapePositionTag.WriteToAsync);
            RegisterParserTag("shape_cache", Primary.And(ArgumentsList), ShapeCacheTag.WriteToAsync);
            RegisterParserTag("shape_tab", Primary.And(Primary), ShapeTabTag.WriteToAsync);
            RegisterParserTag("shape_remove_item", Primary.And(Primary), ShapeRemoveItemTag.WriteToAsync);
            RegisterParserTag("shape_add_properties", Primary.And(ArgumentsList), ShapeAddPropertyTag.WriteToAsync);
            RegisterParserTag("shape_remove_property", Primary.And(Primary), ShapeRemovePropertyTag.WriteToAsync);
            RegisterParserTag("shape_pager", Primary.And(ArgumentsList), ShapePagerTag.WriteToAsync);

            RegisterParserTag("httpcontext_add_items", ArgumentsList, HttpContextAddItemTag.WriteToAsync);
            RegisterParserTag("httpcontext_remove_items", Primary, HttpContextRemoveItemTag.WriteToAsync);

            RegisterParserTag("helper", ArgumentsList, FluidTagHelper.WriteArgumentsTagHelperAsync);
            RegisterParserTag("shape", ArgumentsList, async (list, writer, encoder, context) => await FluidTagHelper.WriteToAsync("shape", list, null, writer, encoder, context));
            RegisterParserTag("contentitem", ArgumentsList, async (list, writer, encoder, context) => await FluidTagHelper.WriteToAsync("contentitem", list, null, writer, encoder, context));
            RegisterParserTag("link", ArgumentsList, async (list, writer, encoder, context) => await FluidTagHelper.WriteToAsync("link", list, null, writer, encoder, context));
            RegisterParserTag("meta", ArgumentsList, async (list, writer, encoder, context) => await FluidTagHelper.WriteToAsync("meta", list, null, writer, encoder, context));
            RegisterParserTag("resources", ArgumentsList, async (list, writer, encoder, context) => await FluidTagHelper.WriteToAsync("resources", list, null, writer, encoder, context));
            RegisterParserTag("script", ArgumentsList, async (list, writer, encoder, context) => await FluidTagHelper.WriteToAsync("script", list, null, writer, encoder, context));
            RegisterParserTag("style", ArgumentsList, async (list, writer, encoder, context) => await FluidTagHelper.WriteToAsync("style", list, null, writer, encoder, context));

            RegisterParserBlock("block", ArgumentsList, FluidTagHelper.WriteArgumentsBlockHelperAsync);
            RegisterParserBlock("a", ArgumentsList, async (list, statements, writer, encoder, context) => await FluidTagHelper.WriteToAsync("a", list, statements, writer, encoder, context));
            RegisterParserBlock("zone", ArgumentsList, async (list, statements, writer, encoder, context) => await FluidTagHelper.WriteToAsync("zone", list, statements, writer, encoder, context));
            RegisterParserBlock("form", ArgumentsList, async (list, statements, writer, encoder, context) => await FluidTagHelper.WriteToAsync("form", list, statements, writer, encoder, context));
            RegisterParserBlock("scriptblock", ArgumentsList, async (list, statements, writer, encoder, context) => await FluidTagHelper.WriteToAsync("scriptblock", list, statements, writer, encoder, context));
            RegisterParserBlock("styleblock", ArgumentsList, async (list, statements, writer, encoder, context) => await FluidTagHelper.WriteToAsync("styleblock", list, statements, writer, encoder, context));

            // Dynamic caching
            RegisterParserBlock("cache", ArgumentsList, CacheTag.WriteToAsync);
            RegisterParserTag("cache_dependency", Primary, CacheDependencyTag.WriteToAsync);
            RegisterParserTag("cache_expires_on", Primary, CacheExpiresOnTag.WriteToAsync);
            RegisterParserTag("cache_expires_after", Primary, CacheExpiresAfterTag.WriteToAsync);
            RegisterParserTag("cache_expires_sliding", Primary, CacheExpiresSlidingTag.WriteToAsync);
        }
    }
}
