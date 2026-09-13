using System.CommandLine;
using System.CommandLine.Parsing;

namespace OrchardCore.Cli;

internal sealed partial class CliApplication
{
    private Command CreateDeviceLoginCommand(Argument<string?> contextArgument)
    {
        var command = new Command("device", "Start, display, and complete a saved device authorization session");
        var start = new Command("start", "Request device authorization and return immediately");
        var startQr = CreateDeviceQrOption();
        start.Options.Add(startQr);
        start.SetAction(async (parseResult, cancellationToken) =>
        {
            var context = RequireContext(parseResult.GetValue(contextArgument) ?? parseResult.GetValue(_contextOption));
            EnsureGrantSupported(context, "device");
            var store = new DeviceLoginSessionStore(_paths);
            await store.PruneExpiredAsync(cancellationToken);
            var discovery = await _oauthClient.GetDiscoveryAsync(GetAuthority(context), cancellationToken);
            var session = await _oauthClient.StartDeviceAuthorizationAsync(context, discovery, cancellationToken);
            await store.SaveAsync(session, cancellationToken);
            return await WriteDeviceSessionAsync(parseResult, session, parseResult.GetValue(startQr), cancellationToken);
        });

        var show = new Command("show", "Display an existing device authorization session without contacting the server");
        var showId = new Argument<string>("session-id");
        var showQr = CreateDeviceQrOption();
        show.Arguments.Add(showId);
        show.Options.Add(showQr);
        show.SetAction(async (parseResult, cancellationToken) =>
        {
            var store = new DeviceLoginSessionStore(_paths);
            var session = await store.ReadAsync(parseResult.GetValue(showId)!, cancellationToken);
            if (session.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                await store.PruneExpiredAsync(cancellationToken);
                throw new CliException("The device login session has expired. Run 'pomi login device start' again.");
            }
            _ = ValidateDeviceSessionContext(session, parseResult, contextArgument);
            return await WriteDeviceSessionAsync(parseResult, session, parseResult.GetValue(showQr), cancellationToken);
        });

        var wait = new Command("wait", "Wait for approval of a saved session and sign in to its original context");
        var waitId = new Argument<string>("session-id");
        wait.Arguments.Add(waitId);
        wait.SetAction(async (parseResult, cancellationToken) =>
        {
            if (!_credentialStore.SupportsPersistentHumanTokens)
            {
                throw new CliException("Credential storage is not available on this platform.");
            }
            var store = new DeviceLoginSessionStore(_paths);
            var id = parseResult.GetValue(waitId)!;
            using var lease = store.Acquire(id);
            var session = await store.ReadAsync(id, cancellationToken);
            var context = ValidateDeviceSessionContext(session, parseResult, contextArgument);
            StoredToken token;
            try
            {
                token = await _oauthClient.WaitForDeviceAuthorizationAsync(session, cancellationToken,
                    ct => store.SaveAsync(session, ct));
            }
            catch (DeviceAuthorizationException)
            {
                store.Delete(id);
                throw;
            }

            // Once the server issues a token, save it before discarding the
            // one-time device code, even if cancellation arrives at this point.
            var latest = await _contextStore.LoadAsync(CancellationToken.None);
            _configuration.CurrentContext = latest.CurrentContext;
            _configuration.Contexts = latest.Contexts;
            try
            {
                context = ValidateDeviceSessionContext(session, parseResult, contextArgument);
            }
            catch (CliException)
            {
                store.Delete(id);
                throw;
            }
            await _credentialStore.SaveAsync(GetCredentialKey(context), token, CancellationToken.None);
            store.Delete(id);
            return await RefreshAndReportLoginAsync(parseResult, context, token, "device", cancellationToken);
        });

        command.Subcommands.Add(start);
        command.Subcommands.Add(show);
        command.Subcommands.Add(wait);
        return command;
    }

    private static Option<QrCodeMode> CreateDeviceQrOption() => new("--qr")
    {
        Description = "Optional QR image: auto, always, never; JSON includes a base64 PNG",
        DefaultValueFactory = _ => QrCodeMode.Never,
    };

    private TenantContextRecord ValidateDeviceSessionContext(DeviceLoginSession session, ParseResult parseResult, Argument<string?> contextArgument)
    {
        var requestedContext = parseResult.GetValue(contextArgument) ?? parseResult.GetValue(_contextOption);
        if (requestedContext is not null && !string.Equals(requestedContext, session.ContextName, StringComparison.OrdinalIgnoreCase))
        {
            throw new CliException($"This device login session belongs to context '{session.ContextName}', not '{requestedContext}'.");
        }
        var context = RequireContext(session.ContextName);
        if (CliPaths.NormalizeTenantUrl(context.TenantUrl) != CliPaths.NormalizeTenantUrl(session.TenantUrl) || context.ClientId != session.ClientId
            || !context.Scopes.Order(StringComparer.Ordinal).SequenceEqual(session.Scopes.Order(StringComparer.Ordinal), StringComparer.Ordinal))
        {
            throw new CliException("The context's tenant, client, or scopes changed after device authorization started. Run 'pomi login device start' again.");
        }
        CliUtilities.EnsureIssuerMatches(GetAuthority(context).AbsoluteUri, session.Issuer);
        CliUriPolicy.RequireSameOrigin(GetAuthority(context), session.TokenEndpoint);
        CliUriPolicy.RequireSameOrigin(GetAuthority(context), session.VerificationUri);
        EnsureGrantSupported(context, "device");
        return context;
    }

    private async Task<int> WriteDeviceSessionAsync(ParseResult parseResult, DeviceLoginSession session, QrCodeMode qr, CancellationToken cancellationToken)
    {
        var format = CliUtilities.ParseOutputFormat(parseResult.GetValue(_outputOption));
        var output = session.ToPublicJson(includeQr: qr != QrCodeMode.Never && format == OutputFormat.Json);
        await WriteOutputAsync(parseResult, CliUtilities.ToJsonElement(output), cancellationToken);
        if (format == OutputFormat.Human)
        {
            await Console.Out.WriteLineAsync($"\nTo complete sign-in: pomi login device wait {session.SessionId}");
            if (TerminalQrCode.Render(session.VerificationUri, TerminalQrCode.GetMaxWidth(qr)) is { } image)
            {
                await Console.Error.WriteAsync(image);
            }
        }
        return 0;
    }

    private async Task<int> RefreshAndReportLoginAsync(ParseResult parseResult, TenantContextRecord context, StoredToken token, string grantType, CancellationToken cancellationToken)
    {
        _ = await RefreshManifestAsync(context, force: true, allowStale: false, cancellationToken);
        _ = await RefreshOpenApiAsync(context, force: true, allowStale: false, cancellationToken);
        return await WriteOutputAsync(parseResult, CliUtilities.ToJsonElement(new LoginOutput
        {
            Context = context.Name,
            GrantType = grantType,
            ExpiresAt = token.ExpiresAt,
            Issuer = token.Issuer,
        }, CliJsonContext.Default.LoginOutput), cancellationToken);
    }
}
