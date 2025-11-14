using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Data.Documents;
using YesSqlSession = YesSql.ISession;

namespace OrchardCore.Data;

/// <summary>
/// Middleware that ensures database commits happen before the HTTP response is sent.
/// </summary>
public class CommitSessionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CommitSessionMiddleware> _logger;

    public CommitSessionMiddleware(RequestDelegate next, ILogger<CommitSessionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<EnsureCommittedOptions> options)
    {
        var ensureCommittedOptions = options?.Value;

        if (ensureCommittedOptions == null || !ensureCommittedOptions.Enabled)
        {
            await _next(context);
            return;
        }

        // Check if we should filter by path
        if (ensureCommittedOptions.FlushOnPaths?.Length > 0)
        {
            var currentPath = context.Request.Path.Value ?? string.Empty;
            var shouldFlush = false;

            foreach (var path in ensureCommittedOptions.FlushOnPaths)
            {
                if (currentPath.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                {
                    shouldFlush = true;
                    break;
                }
            }

            if (!shouldFlush)
            {
                await _next(context);
                return;
            }
        }

        // Register the commit callback to run before the response starts
        try
        {
            context.Response.OnStarting(async () =>
            {
                // Check if already committed
                if (context.Items.ContainsKey("OrchardCore:Committed"))
                {
                    return;
                }

                try
                {
                    // Check if Session was resolved during this request
                    if (context.Items.ContainsKey("OrchardCore:SessionResolved"))
                    {
                        // Try DocumentStore first (it provides better commit semantics with after-commit callbacks)
                        var documentStore = context.RequestServices.GetService<IDocumentStore>();
                        if (documentStore != null)
                        {
                            _logger.LogDebug("Committing IDocumentStore before response");
                            await documentStore.CommitAsync();
                            context.Items["OrchardCore:Committed"] = true;
                        }
                        else
                        {
                            // Fallback to direct session commit
                            var session = context.RequestServices.GetService<YesSqlSession>();
                            if (session != null)
                            {
                                _logger.LogDebug("Committing ISession before response");
                                await session.SaveChangesAsync();
                                context.Items["OrchardCore:Committed"] = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to commit database changes before response");

                    if (ensureCommittedOptions.FailureBehavior == CommitFailureBehavior.ThrowOnCommitFailure)
                    {
                        throw;
                    }
                }
            });
        }
        catch (InvalidOperationException)
        {
            // Response may have already started, which is fine
            _logger.LogDebug("Could not register OnStarting callback - response may have already started");
        }

        await _next(context);
    }
}
