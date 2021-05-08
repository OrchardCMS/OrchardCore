using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Liquid.Tags;
using OrchardCore.DynamicCache.Liquid;
using Parlot.Fluent;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidViewParser : FluidParser
    {
        public LiquidViewParser(IOptions<LiquidViewOptions> liquidViewOptions)
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
            RegisterParserBlock("block", ArgumentsList, FluidTagHelper.WriteArgumentsBlockHelperAsync);

            RegisterParserTag("shape", ArgumentsList, ShapeTag.WriteToAsync);
            RegisterParserBlock("zone", ArgumentsList, ZoneTag.WriteToAsync);

            RegisteredTags["a"] = ArgumentsList.AndSkip(TagEnd).And(Parsers.ZeroOrOne(AnyTagsList.AndSkip(CreateTag("enda")))).Then<Statement>(x => new ParserBlockStatement<List<FilterArgument>>(x.Item1, x.Item2, DefaultAnchorTag.WriteToAsync));
            RegisterParserBlock("form", ArgumentsList, (list, statements, writer, encoder, context) => FluidTagHelper.WriteToAsync("form", list, statements, writer, encoder, context));

            // Dynamic caching
            RegisterParserBlock("cache", ArgumentsList, CacheTag.WriteToAsync);
            RegisterParserTag("cache_dependency", Primary, CacheDependencyTag.WriteToAsync);
            RegisterParserTag("cache_expires_on", Primary, CacheExpiresOnTag.WriteToAsync);
            RegisterParserTag("cache_expires_after", Primary, CacheExpiresAfterTag.WriteToAsync);
            RegisterParserTag("cache_expires_sliding", Primary, CacheExpiresSlidingTag.WriteToAsync);

            foreach (var configuration in liquidViewOptions.Value.LiquidViewParserConfiguration)
            {
                configuration(this);
            }
        }

        public Parser<List<FilterArgument>> ArgumentsListParser => ArgumentsList;

        internal sealed class ParserBlockStatement<T> : TagStatement
        {
            private readonly Func<T, IReadOnlyList<Statement>, TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> _render;

            public ParserBlockStatement(T value, List<Statement> statements, Func<T, IReadOnlyList<Statement>, TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> render) : base(statements)
            {
                Value = value;
                _render = render ?? throw new ArgumentNullException(nameof(render));
            }

            public T Value { get; }

            public override ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
            {
                return _render(Value, Statements, writer, encoder, context);
            }
        }
    }
}
