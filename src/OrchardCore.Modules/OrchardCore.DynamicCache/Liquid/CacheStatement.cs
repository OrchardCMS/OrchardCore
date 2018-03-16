using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.Extensions.DependencyInjection;
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

            if (!context.AmbientValues.TryGetValue("Services", out var servicesObj))
            {
                throw new ArgumentException("Services missing while invoking 'cache' block");
            }
            
            var services = servicesObj as IServiceProvider;
            
            var dynamicCache = services.GetService<IDynamicCacheService>(); // todo: if this is not registered then just fall through without caching?
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
            
            var cacheResult = await dynamicCache.GetCachedValueAsync(cacheContext.CacheId);
            if (cacheResult != null)
            {
                await writer.WriteAsync(cacheResult);

                return Completion.Normal;
            }

            var content = new StringWriter();

            cacheScopeManager.EnterScope(cacheContext);

            foreach (var statement in Statements)
            {
                await statement.WriteToAsync(content, encoder, context);
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
        }
    }
}