using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.Environment.Cache;
using OrchardCore.Liquid;

namespace OrchardCore.DynamicCache.Liquid
{
    public class CacheTag
    {
        private static readonly char[] _splitChars = new[] { ',', ' ' };

        public static async ValueTask<Completion> WriteToAsync(List<FilterArgument> arguments, IReadOnlyList<Statement> statements, TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var services = ((LiquidTemplateContext)context).Services;

            var dynamicCache = services.GetService<IDynamicCacheService>();
            var cacheScopeManager = services.GetService<ICacheScopeManager>();
            var loggerFactory = services.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<CacheTag>();
            var cacheOptions = services.GetRequiredService<IOptions<CacheOptions>>().Value;

            if (dynamicCache == null || cacheScopeManager == null)
            {
                logger.LogInformation(@"Liquid cache block entered without an available IDynamicCacheService or ICacheScopeManager.
                                        The contents of the cache block will not be cached.
                                        To enable caching, make sure that a feature that contains an implementation of IDynamicCacheService and ICacheScopeManager is enabled (for example, 'Dynamic Cache').");

                if (statements != null && statements.Count > 0)
                {
                    var completion = await statements.RenderStatementsAsync(writer, encoder, context);

                    if (completion != Completion.Normal)
                    {
                        return completion;
                    }
                }

                return Completion.Normal;
            }

            var filterArguments = new FilterArguments();
            foreach (var argument in arguments)
            {
                filterArguments.Add(argument.Name, await argument.Expression.EvaluateAsync(context));
            }

            var cacheKey = filterArguments.At(0).ToStringValue();
            var contexts = filterArguments["vary_by"].ToStringValue();
            var tags = filterArguments["dependencies"].ToStringValue();
            var durationString = filterArguments["expires_after"].ToStringValue();
            var slidingDurationString = filterArguments["expires_sliding"].ToStringValue();

            var cacheContext = new CacheContext(cacheKey)
                .AddContext(contexts.Split(_splitChars, StringSplitOptions.RemoveEmptyEntries))
                .AddTag(tags.Split(_splitChars, StringSplitOptions.RemoveEmptyEntries));

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

            var content = "";
            try
            {
                if (statements != null && statements.Count > 0)
                {
                    using var sb = StringBuilderPool.GetInstance();
                    using (var render = new StringWriter(sb.Builder))
                    {
                        foreach (var statement in statements)
                        {
                            await statement.WriteToAsync(render, encoder, context);
                        }

                        await render.FlushAsync();
                    }

                    content = sb.Builder.ToString();
                }
            }
            finally
            {
                cacheScopeManager.ExitScope();
            }

            if (cacheOptions.DebugMode)
            {
                // No need to optimize this code as it will be used for debugging purpose.
                var debugContent = new StringWriter();
                debugContent.WriteLine();
                debugContent.WriteLine($"<!-- CACHE BLOCK: {cacheContext.CacheId} ({Guid.NewGuid()})");
                debugContent.WriteLine($"         VARY BY: {String.Join(", ", cacheContext.Contexts)}");
                debugContent.WriteLine($"    DEPENDENCIES: {String.Join(", ", cacheContext.Tags)}");
                debugContent.WriteLine($"      EXPIRES ON: {cacheContext.ExpiresOn}");
                debugContent.WriteLine($"   EXPIRES AFTER: {cacheContext.ExpiresAfter}");
                debugContent.WriteLine($" EXPIRES SLIDING: {cacheContext.ExpiresSliding}");
                debugContent.WriteLine("-->");

                debugContent.WriteLine(content);

                debugContent.WriteLine();
                debugContent.WriteLine($"<!-- END CACHE BLOCK: {cacheContext.CacheId} -->");

                content = debugContent.ToString();
            }

            await dynamicCache.SetCachedValueAsync(cacheContext, content);

            await writer.WriteAsync(content);

            return Completion.Normal;
        }
    }
}
