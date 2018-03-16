using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DynamicCache.Services;
using OrchardCore.Environment.Cache;
using OrchardCore.Liquid.Ast;

namespace OrchardCore.DynamicCache.Liquid
{
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
            var splitChars = new[] { ',', ' ' };

            var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();
            var cacheKey = arguments.At(0).ToStringValue();
            var contexts = arguments["contexts"].ToStringValue();
            var tags = arguments["tags"].ToStringValue();
            var dependencies = arguments["dependencies"].ToStringValue();
            var durationString = arguments["fixed_duration"].ToStringValue();
            var slidingDurationString = arguments["sliding_duration"].ToStringValue();

            //if (!context.AmbientValues.TryGetValue("ViewContext", out var viewContextObj))
            //{
            //    throw new ArgumentException("ViewContext missing while invoking 'cache' block");
            //}

            if (!context.AmbientValues.TryGetValue("Services", out var servicesObj))
            {
                throw new ArgumentException("Services missing while invoking 'cache' block");
            }

            //var viewContext = viewContextObj as ViewContext;
            var services = servicesObj as IServiceProvider;
            
            var dynamicCache = services.GetService<IDynamicCacheService>(); // todo: if this is not registered then just fall through without caching?
            //var shapeFactory = services.GetService<IShapeFactory>();
            //var displayHelperFactory = services.GetService<IDisplayHelperFactory>();
            var cacheScopeManager = services.GetService<ICacheScopeManager>();

            var cacheContext = new CacheContext(cacheKey)
                .AddContext(contexts.Split(splitChars, StringSplitOptions.RemoveEmptyEntries))
                .AddTag(tags.Split(splitChars, StringSplitOptions.RemoveEmptyEntries))
                .AddDependency(dependencies.Split(splitChars, StringSplitOptions.RemoveEmptyEntries));

            if (TimeSpan.TryParse(durationString, out var duration))
            {
                cacheContext.WithDuration(duration);
            }

            if (TimeSpan.TryParse(slidingDurationString, out var slidingDuration))
            {
                cacheContext.WithSlidingExpiration(slidingDuration);
            }

            var cacheResult = "hello";

            cacheResult = await dynamicCache.GetCachedValueAsync(cacheContext.CacheId);
            if (cacheResult != null)
            {
                await writer.WriteAsync(cacheResult);

                return Completion.Normal;
            }
            
            //return Task.FromResult("world");

            //if (!(Statements?.Any() ?? false))
            //{
            //    return Task.FromResult("no statements");
            //    //return null;
            //}

            var content = new StringWriter();

            cacheScopeManager.EnterScope(cacheContext);

            foreach (var statement in Statements)
            {
                var completion = await statement.WriteToAsync(content, encoder, context);

                //if (completion != Completion.Normal)
                //{
                //    return completion;
                //}
            }

            cacheScopeManager.ExitScope();

            if (debugMode)
            {
                var debugContent = new StringWriter();
                debugContent.WriteLine($"<!-- CACHE BLOCK: {cacheContext.CacheId} ({Guid.NewGuid()})");
                debugContent.WriteLine($"        CONTEXTS: {String.Join(", ", cacheContext.Contexts)}");
                debugContent.WriteLine($"            TAGS: {String.Join(", ", cacheContext.Tags)}");
                debugContent.WriteLine($"          DURING: {cacheContext.Duration}");
                debugContent.WriteLine($"         SLIDING: {cacheContext.SlidingExpirationWindow}");
                debugContent.WriteLine("-->");
                debugContent.WriteLine(content.ToString());
                debugContent.WriteLine($"<!-- END CACHE BLOCK: {cacheContext.CacheId} -->");

                content = debugContent;
            }

            await dynamicCache.SetCachedValueAsync(cacheContext, content.ToString());

            await writer.WriteAsync(content.ToString());

            return Completion.Normal;

            //var shape = await shapeFactory.CreateAsync("CacheBlock");
            
            //var metadata = shape.Metadata;

            //metadata.OnProcessing(async s =>
            //{
            //    cacheScopeManager.EnterScope(metadata.Cache());

            //    try
            //    {
            //        if (debugMode)
            //        {
            //            metadata.Wrappers.Add("CacheBlockWrapper");
            //        }
            //    }
            //    finally
            //    {
            //        cacheScopeManager.ExitScope();
            //    }

            //});

            //var display = (DisplayHelper)displayHelperFactory.CreateHelper(viewContext);

            //writer.Write(await display.ShapeExecuteAsync(shape));

            //return Completion.Normal;
        }
    }
}