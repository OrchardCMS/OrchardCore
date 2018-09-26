using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Cache;
using OrchardCore.Liquid.Ast;

namespace OrchardCore.DynamicCache.Liquid
{
    public class CacheStatement : TagStatement
    {
        private static readonly char[] SplitChars = new [] { ',', ' ' };
        private readonly ArgumentsExpression _arguments;

        public CacheStatement(ArgumentsExpression arguments, IList<Statement> statements = null) : base(statements)
        {
            _arguments = arguments;
        }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var servicesObj))
            {
                throw new ArgumentException("Services missing while invoking 'cache' block");
            }

            var services = servicesObj as IServiceProvider;

            var dynamicCache = services.GetService<IDynamicCacheService>();
            var cacheScopeManager = services.GetService<ICacheScopeManager>();
            var loggerFactory = services.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<CacheStatement>();

            if (dynamicCache == null || cacheScopeManager == null)
            {
                logger.LogInformation(@"Liquid cache block entered without an available IDynamicCacheService or ICacheScopeManager. 
                                        The contents of the cache block will not be cached. 
                                        To enable caching, make sure that a feature that contains an implementation of IDynamicCacheService and ICacheScopeManager is enabled (for example, 'Dynamic Cache').");

                await writer.WriteAsync(await EvaluateStatementsAsync(encoder, context));

                return Completion.Normal;
            }
            
            // TODO: Create a configuration setting in the UI
            var debugMode = false;

            var arguments = (FilterArguments)(await _arguments.EvaluateAsync(context)).ToObjectValue();
            var cacheKey = arguments.At(0).ToStringValue();
            var contexts = arguments["vary_by"].ToStringValue();
            var tags = arguments["dependencies"].ToStringValue();
            var durationString = arguments["expires_after"].ToStringValue();
            var slidingDurationString = arguments["expires_sliding"].ToStringValue();

            var cacheContext = new CacheContext(cacheKey)
                .AddContext(contexts.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries))
                .AddTag(tags.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries));

            if (TimeSpan.TryParse(durationString, out var duration))
            {
                cacheContext.WithExpiryAfter(duration);
            }

            if (TimeSpan.TryParse(slidingDurationString, out var slidingDuration))
            {
                cacheContext.WithExpirySliding(slidingDuration);
            }
            
            var cacheResult = await dynamicCache.GetCachedValueAsync(cacheContext);
            if (cacheResult != null)
            {
                await writer.WriteAsync(cacheResult);

                return Completion.Normal;
            }
            
            cacheScopeManager.EnterScope(cacheContext);
            String content;

            try
            {
                content = await EvaluateStatementsAsync(encoder, context);
            }
            finally
            {
                cacheScopeManager.ExitScope();
            }

            if (debugMode)
            {
                var debugContent = new StringWriter();
                debugContent.WriteLine($"<!-- CACHE BLOCK: {cacheContext.CacheId} ({Guid.NewGuid()})");
                debugContent.WriteLine($"         VARY BY: {String.Join(", ", cacheContext.Contexts)}");
                debugContent.WriteLine($"    DEPENDENCIES: {String.Join(", ", cacheContext.Tags)}");
                debugContent.WriteLine($"      EXPIRES ON: {cacheContext.ExpiresOn}");
                debugContent.WriteLine($"   EXPIRES AFTER: {cacheContext.ExpiresAfter}");
                debugContent.WriteLine($" EXPIRES SLIDING: {cacheContext.ExpiresSliding}");
                debugContent.WriteLine("-->");
                debugContent.WriteLine(content);
                debugContent.WriteLine($"<!-- END CACHE BLOCK: {cacheContext.CacheId} -->");

                content = debugContent.ToString();
            }

            await dynamicCache.SetCachedValueAsync(cacheContext, content);
            
            await writer.WriteAsync(content);

            return Completion.Normal;
        }

        private async Task<string> EvaluateStatementsAsync(TextEncoder encoder, TemplateContext context)
        {
            var content = new StringWriter();
            
            foreach (var statement in Statements)
            {
                await statement.WriteToAsync(content, encoder, context);
            }

            return content.ToString();
        }
    }
}