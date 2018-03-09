using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Liquid.Ast;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DisplayManagement.Liquid.Blocks
{
    public class CacheBlock : ArgumentsBlock
    {
        public override Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, FilterArgument[] arguments, IList<Statement> statements)
        {
            var exp = new ArgumentsExpression(arguments);
            var sta = new CacheStatement(exp, statements);
            return sta.WriteToAsync(writer, encoder, context);
        }
    }

    public class CacheStatement : TagStatement
    {
        private readonly ArgumentsExpression _arguments;

        public CacheStatement(ArgumentsExpression arguments, IList<Statement> statements = null) : base(statements)
        {
            _arguments = arguments;
        }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            // TODO: make this configurable
            var debugMode = true;

            var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();
            var cacheKey = arguments.At(0).ToStringValue();
            var contexts = arguments["contexts"].ToStringValue();
            var tags = arguments["tags"].ToStringValue();
            var dependencies = arguments["dependencies"].ToStringValue();
            var durationString = arguments["fixed_duration"].ToStringValue();
            var slidingDurationString = arguments["sliding_duration"].ToStringValue();

            if (!context.AmbientValues.TryGetValue("ViewContext", out var viewContextObj))
            {
                throw new ArgumentException("ViewContext missing while invoking 'cache' block");
            }

            if (!context.AmbientValues.TryGetValue("Services", out var servicesObj))
            {
                throw new ArgumentException("Services missing while invoking 'cache' block");
            }

            var viewContext = viewContextObj as ViewContext;
            var services = servicesObj as IServiceProvider;

            var shapeFactory = services.GetService<IShapeFactory>();
            var displayHelperFactory = services.GetService<IDisplayHelperFactory>();
            var cacheScopeManager = services.GetService<ICacheScopeManager>();

            var shape = await shapeFactory.CreateAsync("CacheBlock");
            
            var metadata = shape.Metadata;
            var splitChars = new[] {',', ' '};
            
            metadata.Cache(cacheKey)
                .AddContext(contexts.Split(splitChars, StringSplitOptions.RemoveEmptyEntries))
                .AddTag(tags.Split(splitChars, StringSplitOptions.RemoveEmptyEntries))
                .AddDependency(dependencies.Split(splitChars, StringSplitOptions.RemoveEmptyEntries));

            if (TimeSpan.TryParse(durationString, out var duration))
            {
                metadata.Cache().WithDuration(duration);
            }

            if (TimeSpan.TryParse(slidingDurationString, out var slidingDuration))
            {
                metadata.Cache().WithSlidingExpiration(slidingDuration);
            }

            metadata.OnProcessing(async s =>
            {
                cacheScopeManager.EnterScope(metadata.Cache());

                try
                {
                    if (debugMode)
                    {
                        metadata.Wrappers.Add("CacheBlockWrapper");
                    }
                    
                    if (Statements?.Any() ?? false)
                    {
                        var content = new StringWriter();
                        foreach (var statement in Statements)
                        {
                            var completion = await statement.WriteToAsync(content, encoder, context);

                            //if (completion != Completion.Normal)
                            //{
                            //    return completion;
                            //}
                        }

                        ((dynamic)shape).ChildContent = content.ToString();
                    }
                }
                finally
                {
                    cacheScopeManager.ExitScope();
                }

            });

            var display = (DisplayHelper)displayHelperFactory.CreateHelper(viewContext);

            writer.Write(await display.ShapeExecuteAsync(shape));

            return Completion.Normal;
        }
    }
}
