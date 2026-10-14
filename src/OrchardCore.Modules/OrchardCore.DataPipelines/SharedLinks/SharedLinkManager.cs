using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;
using OrchardCore.Modules;
using OrchardCore.Users;
using YesSql;
using IIdGenerator = OrchardCore.Entities.IIdGenerator;
using ISession = YesSql.ISession;

namespace OrchardCore.DataPipelines.SharedLinks;

/// <inheritdoc />
public sealed class SharedLinkManager : ISharedLinkManager
{
    private readonly ISession _session;
    private readonly SharedLinkUseCounter _useCounter;
    private readonly IClock _clock;
    private readonly IIdGenerator _idGenerator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _services;
    private readonly HtmlEncoder _htmlEncoder;
    private readonly IStringLocalizer S;

    public SharedLinkManager(
        ISession session,
        SharedLinkUseCounter useCounter,
        IClock clock,
        IIdGenerator idGenerator,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider services,
        HtmlEncoder htmlEncoder,
        IStringLocalizer<SharedLinkManager> localizer)
    {
        _session = session;
        _useCounter = useCounter;
        _clock = clock;
        _idGenerator = idGenerator;
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
        _services = services;
        _htmlEncoder = htmlEncoder;
        S = localizer;
    }

    /// <inheritdoc />
    public async Task<SharedLinkResult> CreateAsync(SharedLinkRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(request.Kind);
        ArgumentException.ThrowIfNullOrEmpty(request.ResourceId);

        if (request.Lifetime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(request), S["A link works for a positive duration."]);
        }

        var emailService = request.NotifyByEmail
            ? _services.GetService<IEmailService>() ?? throw new InvalidOperationException(S["Enable the Email feature to email the shared links."])
            : null;

        var recipients = await ResolveRecipientsAsync(request.Recipients);

        if (recipients.Count == 0)
        {
            throw new InvalidOperationException(S["A link is shared with one user of the site at least."]);
        }

        var token = SharedLinkAccess.CreateToken();
        var now = _clock.UtcNow;
        var link = new SharedLink
        {
            LinkId = _idGenerator.GenerateUniqueId(),
            Kind = request.Kind,
            ResourceId = request.ResourceId,
            Title = request.Title,
            SourceName = request.SourceName,
            Properties = new Dictionary<string, string>(request.Properties ?? [], StringComparer.Ordinal),
            TokenHash = SharedLinkAccess.Hash(token),
            RecipientUserIds = recipients.Select(recipient => recipient.UserId).ToList(),
            RecipientNames = recipients.Select(recipient => recipient.UserName).ToList(),
            CreatedUtc = now,
            ExpiresUtc = now.Add(request.Lifetime),
            MaxUses = Math.Max(0, request.MaxUses),
        };

        await _session.SaveAsync(link, cancellationToken: cancellationToken);
        await _useCounter.AddAsync(link.LinkId);

        var url = GetUrl(link.LinkId, token);
        var notified = emailService is null ? [] : await NotifyAsync(emailService, recipients, link, url, request, cancellationToken);

        return new SharedLinkResult
        {
            Link = link,
            Url = url,
            Notified = notified,
        };
    }

    /// <inheritdoc />
    public async Task<SharedLink> GetAsync(string linkId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(linkId))
        {
            return null;
        }

        var link = await _session.Query<SharedLink, SharedLinkIndex>(index => index.LinkId == linkId).FirstOrDefaultAsync(cancellationToken);

        if (link is not null)
        {
            await LoadUsesAsync([link]);
        }

        return link;
    }

    /// <inheritdoc />
    public async Task<(SharedLinkAccessResult Result, SharedLink Link)> UseAsync(string linkId, string token, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var link = await GetAsync(linkId, cancellationToken);
        var now = _clock.UtcNow;
        var result = SharedLinkAccess.Check(link, token, user, now);

        if (result != SharedLinkAccessResult.Allowed)
        {
            return (result, link);
        }

        // The database grants the use: the count read with the link may already be stale.
        var (granted, uses) = await _useCounter.TryUseAsync(link.LinkId, link.MaxUses);
        link.Uses = uses;

        if (!granted)
        {
            return (SharedLinkAccessResult.UsedUp, link);
        }

        link.LastUsedUtc = now;
        await _session.SaveAsync(link, cancellationToken: cancellationToken);

        // Committed right away, so the counter isn't locked while the resource is served.
        await _session.SaveChangesAsync(cancellationToken);

        return (result, link);
    }

    /// <inheritdoc />
    public async Task RevokeAsync(SharedLink link, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(link);

        link.RevokedUtc ??= _clock.UtcNow;

        await _session.SaveAsync(link, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(SharedLink link, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(link);

        var handler = FindHandler(link.Kind);

        if (handler is not null)
        {
            await handler.DeletedAsync(link);
        }

        _session.Delete(link);
        await _useCounter.DeleteAsync(link.LinkId);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<SharedLink> Links, int Count)> ListAsync(string search, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _session.Query<SharedLink, SharedLinkIndex>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var text = search.Trim();
            query = query.Where(index => index.Title.Contains(text) || index.SourceName.Contains(text));
        }

        var count = await query.CountAsync(cancellationToken);
        var links = (await query.OrderByDescending(index => index.CreatedUtc).Skip(skip).Take(take).ListAsync(cancellationToken)).ToList();

        await LoadUsesAsync(links);

        return (links, count);
    }

    /// <summary>
    /// Lists the links that stopped working before a date.
    /// </summary>
    /// <param name="before">The date, in UTC.</param>
    /// <param name="take">The number of links to return.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The links.</returns>
    public async Task<IReadOnlyList<SharedLink>> ListExpiredAsync(DateTime before, int take, CancellationToken cancellationToken = default)
    {
        return (await _session.Query<SharedLink, SharedLinkIndex>(index => index.ExpiresUtc < before).Take(take).ListAsync(cancellationToken)).ToList();
    }

    /// <summary>
    /// Finds the handler of a kind of resource.
    /// </summary>
    /// <param name="kind">The kind of resource.</param>
    /// <returns>The handler, or <see langword="null"/> when the feature that registered it is disabled.</returns>
    public ISharedLinkHandler FindHandler(string kind)
    {
        return _services.GetServices<ISharedLinkHandler>().FirstOrDefault(handler => string.Equals(handler.Kind, kind, StringComparison.Ordinal));
    }

    // The counter of the uses is the reference, the copy in the document of a link may be stale.
    private async Task LoadUsesAsync(IReadOnlyCollection<SharedLink> links)
    {
        var uses = await _useCounter.GetUsesAsync(links.Select(link => link.LinkId).ToList());

        foreach (var link in links)
        {
            if (uses.TryGetValue(link.LinkId, out var count))
            {
                link.Uses = count;
            }
        }
    }

    private async Task<List<Recipient>> ResolveRecipientsAsync(IReadOnlyList<string> names)
    {
        var userManager = _services.GetService<UserManager<IUser>>()
            ?? throw new InvalidOperationException(S["Enable the Users feature to share links with users."]);

        var recipients = new List<Recipient>();

        foreach (var name in names ?? [])
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var user = await userManager.FindByNameAsync(name.Trim()) ?? await userManager.FindByEmailAsync(name.Trim())
                ?? throw new InvalidOperationException(S["There is no user named '{0}', or with that email.", name.Trim()]);

            var userId = await userManager.GetUserIdAsync(user);

            if (recipients.All(recipient => recipient.UserId != userId))
            {
                recipients.Add(new Recipient(userId, await userManager.GetUserNameAsync(user), await userManager.GetEmailAsync(user)));
            }
        }

        return recipients;
    }

    private string GetUrl(string linkId, string token)
    {
        var values = new RouteValueDictionary { ["area"] = "OrchardCore.DataPipelines", ["linkId"] = linkId, ["token"] = token };
        var httpContext = _httpContextAccessor.HttpContext;

        return (httpContext is null ? null : _linkGenerator.GetUriByAction(httpContext, "Open", "SharedLink", values))
            ?? _linkGenerator.GetPathByAction("Open", "SharedLink", values);
    }

    private async Task<IReadOnlyList<string>> NotifyAsync(
        IEmailService emailService,
        List<Recipient> recipients,
        SharedLink link,
        string url,
        SharedLinkRequest request,
        CancellationToken cancellationToken)
    {
        var title = link.Title ?? link.ResourceId;
        var body = $"""
            <p>{_htmlEncoder.Encode(S["'{0}' was shared with you.", title])}</p>
            {(string.IsNullOrWhiteSpace(request.Message) ? string.Empty : $"<p>{_htmlEncoder.Encode(request.Message)}</p>")}
            <p><a href="{_htmlEncoder.Encode(url)}">{_htmlEncoder.Encode(S["Open {0}", title])}</a></p>
            <p>{_htmlEncoder.Encode(UsageText(link))}</p>
            """;

        var notified = new List<string>();

        foreach (var recipient in recipients.Where(recipient => !string.IsNullOrEmpty(recipient.Email)))
        {
            var result = await emailService.SendAsync(new MailMessage
            {
                To = recipient.Email,
                Subject = string.IsNullOrWhiteSpace(request.Subject) ? S["'{0}' was shared with you", title] : request.Subject,
                HtmlBody = body,
            }, cancellationToken: cancellationToken);

            if (result.Succeeded)
            {
                notified.Add(recipient.UserName);
            }
        }

        return notified;
    }

    private string UsageText(SharedLink link)
    {
        var expires = link.ExpiresUtc.ToString("f", CultureInfo.CurrentCulture);

        return link.MaxUses > 0
            ? S.Plural(link.MaxUses, "Sign in to open it. The link works once, until {1} (UTC).", "Sign in to open it. The link works {0} times, until {1} (UTC).", expires)
            : S["Sign in to open it. The link works until {0} (UTC).", expires];
    }

    private sealed record Recipient(string UserId, string UserName, string Email);
}
