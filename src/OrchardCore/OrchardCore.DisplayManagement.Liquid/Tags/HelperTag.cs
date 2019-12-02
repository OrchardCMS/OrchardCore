using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Liquid.TagHelpers;
using OrchardCore.Liquid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class HelperTag : ArgumentsTag
    {
        public override ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, FilterArgument[] arguments)
        {
            return new HelperStatement(new ArgumentsExpression(arguments)).WriteToAsync(writer, encoder, context);
        }
    }

    public class HelperBlock : ArgumentsBlock
    {
        public override ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, FilterArgument[] arguments, List<Statement> statements)
        {
            return new HelperStatement(new ArgumentsExpression(arguments), null, statements).WriteToAsync(writer, encoder, context);
        }
    }

    public class HelperStatement : TagStatement
    {
        private LiquidTagHelperActivator _activator;
        private readonly ArgumentsExpression _arguments;
        private readonly string _helper;

        public HelperStatement(ArgumentsExpression arguments, string helper = null, List<Statement> statements = null) : base(statements)
        {
            _arguments = arguments;
            _helper = helper;
        }

        public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var servicesValue))
            {
                throw new ArgumentException("Services missing while invoking 'helper'");
            }

            var services = servicesValue as IServiceProvider;

            var viewContextAccessor = services.GetRequiredService<ViewContextAccessor>();
            var viewContext = viewContextAccessor.ViewContext;

            var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();
            var helper = _helper ?? arguments["helper_name"].Or(arguments.At(0)).ToStringValue();

            var factory = services.GetRequiredService<LiquidTagHelperFactory>();

            // Each tag is a singleton, as views are
            if (_activator == null)
            {
                lock (this)
                {
                    if (_activator == null)
                    {
                        _activator = factory.GetActivator(helper, arguments.Names);
                    }
                }
            }

            if (_activator == LiquidTagHelperActivator.None)
            {
                return Completion.Normal;
            }

            var tagHelper = factory.CreateTagHelper(_activator, (ViewContext)viewContext,
                arguments, out var contextAttributes, out var outputAttributes);

            var content = "";

            // Build the ChildContent of this tag
            using (var sb = StringBuilderPool.GetInstance())
            {
                using (var output = new StringWriter(sb.Builder))
                {
                    if (Statements != null && Statements.Count > 0)
                    {
                        var completion = Completion.Break;

                        for (var index = 0; index < Statements.Count; index++)
                        {
                            completion = await Statements[index].WriteToAsync(output, encoder, context);

                            if (completion != Completion.Normal)
                            {
                                return completion;
                            }
                        }
                    }

                    await output.FlushAsync();
                }

                content = sb.Builder.ToString();
            }

            var tagHelperContext = new TagHelperContext(contextAttributes, new Dictionary<object, object>(), Guid.NewGuid().ToString("N"));

            var tagHelperOutput = new TagHelperOutput(helper, outputAttributes, (_, e)
                => Task.FromResult(new DefaultTagHelperContent().AppendHtml(content)));

            tagHelperOutput.Content.AppendHtml(content);

            await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

            tagHelperOutput.WriteTo(writer, (HtmlEncoder)encoder);

            return Completion.Normal;
        }
    }
}
