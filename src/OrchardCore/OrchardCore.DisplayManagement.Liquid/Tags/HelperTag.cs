using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Liquid.Ast;
using OrchardCore.DisplayManagement.Liquid.Tags.TagHelpers;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class HelperTag : ArgumentsTag
    {
        public override Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, FilterArgument[] arguments)
        {
            return new HelperStatement(new ArgumentsExpression(arguments)).WriteToAsync(writer, encoder, context);
        }
    }

    public class HelperBlock : ArgumentsBlock
    {
        public override Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, FilterArgument[] arguments, IList<Statement> statements)
        {
            return new HelperStatement(new ArgumentsExpression(arguments), null, statements).WriteToAsync(writer, encoder, context);
        }
    }

    public class HelperStatement : TagStatement
    {
        private const string AspPrefix = "asp-";

        private TagHelperMatching _matching;
        private readonly ArgumentsExpression _arguments;
        private readonly string _helper;

        public HelperStatement(ArgumentsExpression arguments, string helper = null, IList<Statement> statements = null) : base(statements)
        {
            _arguments = arguments;
            _helper = helper;
        }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var servicesValue))
            {
                throw new ArgumentException("Services missing while invoking 'helper'");
            }

            var services = servicesValue as IServiceProvider;

            if (!context.AmbientValues.TryGetValue("ViewContext", out var viewContext))
            {
                throw new ArgumentException("ViewContext missing while invoking 'helper'");
            }

            var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();

            var helper = _helper ?? arguments["helper_name"].Or(arguments.At(0)).ToStringValue();
            var tagHelperSharedState = services.GetRequiredService<TagHelperSharedState>();

            if (_matching == null)
            {
                lock (this)
                {
                    if (_matching == null)
                    {
                        _matching = tagHelperSharedState.GetMatching(helper, arguments.Names);
                    }
                }
            }

            if (_matching == TagHelperMatching.None)
            {
                return Completion.Normal;
            }

            var factory = services.GetRequiredService<ITagHelperFactory>();
            var activator = TagHelperSharedState.GetActivator(_matching);

            var tagHelper = activator.CreateTagHelper(factory, (ViewContext)viewContext,
                arguments, out var contextAttributes, out var outputAttributes);

            var content = new StringWriter();
            if (Statements?.Any() ?? false)
            {
                Completion completion = Completion.Break;
                for (var index = 0; index < Statements.Count; index++)
                {
                    completion = await Statements[index].WriteToAsync(content, encoder, context);

                    if (completion != Completion.Normal)
                    {
                        return completion;
                    }
                }
            }

            var tagHelperContext = new TagHelperContext(contextAttributes,
                new Dictionary<object, object>(), Guid.NewGuid().ToString("N"));

            var tagHelperOutput = new TagHelperOutput(helper, outputAttributes, (_, e)
                => Task.FromResult(new DefaultTagHelperContent().AppendHtml(content.ToString())));

            tagHelperOutput.Content.AppendHtml(content.ToString());
            await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

            tagHelperOutput.WriteTo(writer, HtmlEncoder.Default);

            return Completion.Normal;
        }
    }
}
