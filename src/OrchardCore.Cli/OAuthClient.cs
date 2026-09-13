using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace OrchardCore.Cli;

internal sealed class OAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly TextWriter _stderr;
    private readonly Action<string> _launchBrowser;

    public OAuthClient(HttpClient httpClient, TextWriter stderr, Action<string>? launchBrowser = null)
    {
        _httpClient = httpClient;
        _stderr = stderr;
        _launchBrowser = launchBrowser ?? LaunchBrowser;
    }

    public async Task<OidcDiscoveryDocument> GetDiscoveryAsync(Uri authority, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authority);

        CliUriPolicy.RequireSecureEndpoint(authority.AbsoluteUri);
        var discoveryUri = new Uri($"{authority.AbsoluteUri.TrimEnd('/')}/.well-known/openid-configuration");
        using var response = await _httpClient.GetAsync(discoveryUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var discovery = CliUtilities.ParseDiscoveryDocument(content);
        CliUtilities.EnsureIssuerMatches(authority.AbsoluteUri, discovery.Issuer);
        CliUriPolicy.RequireSameOrigin(authority, discovery.TokenEndpoint);
        foreach (var endpoint in new[] { discovery.AuthorizationEndpoint, discovery.DeviceAuthorizationEndpoint, discovery.RevocationEndpoint })
        {
            if (endpoint is not null)
            {
                CliUriPolicy.RequireSameOrigin(authority, endpoint);
            }
        }
        return discovery;
    }

    public async Task<StoredToken> LoginWithAuthorizationCodeAsync(TenantContextRecord context, OidcDiscoveryDocument discovery, CancellationToken cancellationToken, bool openBrowser = true)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(discovery);

        var clientId = context.ClientId ?? throw new CliException("The selected context does not declare a CLI client identifier.");
        var authorizationEndpoint = discovery.AuthorizationEndpoint ??
            throw new CliException("The identity provider does not expose an authorization endpoint.");
        var scope = string.Join(' ', context.Scopes.Count > 0 ? context.Scopes : ["openid", "profile", "offline_access"]);
        var state = CreateRandomString();
        var codeVerifier = CreateRandomString();
        var challenge = CliUtilities.Base64UrlEncode(SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier)));

        await using var listener = await LoopbackCallbackListener.StartAsync(cancellationToken);
        var authorizationUri = BuildAuthorizationUri(authorizationEndpoint, clientId, listener.RedirectUri, scope, state, challenge);
        if (openBrowser)
        {
            _launchBrowser(authorizationUri);
            await _stderr.WriteLineAsync($"Opening browser for '{context.Name}' login...");
        }
        else
        {
            await _stderr.WriteLineAsync($"Open this URL in a browser on this computer: {authorizationUri}");
        }

        var (code, returnedState) = await listener.WaitForCallbackAsync(state, discovery.Issuer, cancellationToken);
        if (!string.Equals(returnedState, state, StringComparison.Ordinal))
        {
            throw new CliException("OAuth state validation failed.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, discovery.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = clientId,
                ["code"] = code,
                ["redirect_uri"] = listener.RedirectUri,
                ["code_verifier"] = codeVerifier,
            }),
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();
        var tokenResponse = CliUtilities.ParseTokenResponse(responseContent);
        return CliUtilities.CreateStoredToken(tokenResponse, discovery);
    }

    public async Task<StoredToken> LoginWithDeviceCodeAsync(TenantContextRecord context, OidcDiscoveryDocument discovery, CancellationToken cancellationToken, int qrCodeWidth = 0, TextWriter? qrCodeJsonWriter = null)
    {
        var session = await StartDeviceAuthorizationAsync(context, discovery, cancellationToken);
        await _stderr.WriteLineAsync($"Open {session.VerificationUri} and enter code {session.UserCode}.");
        if (qrCodeJsonWriter is not null)
        {
            await qrCodeJsonWriter.WriteLineAsync(session.ToPublicJson(includeQr: true, includeSessionId: false).ToJsonString());
            await qrCodeJsonWriter.FlushAsync(cancellationToken);
        }
        else if (TerminalQrCode.Render(session.VerificationUri, qrCodeWidth) is { } qrCode)
        {
            await _stderr.WriteLineAsync("Scan to sign in on another device, then verify the code:");
            await _stderr.WriteAsync(qrCode);
        }

        return await WaitForDeviceAuthorizationAsync(session, cancellationToken);
    }

    public async Task<DeviceLoginSession> StartDeviceAuthorizationAsync(TenantContextRecord context, OidcDiscoveryDocument discovery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(discovery);
        if (string.IsNullOrWhiteSpace(discovery.DeviceAuthorizationEndpoint))
        {
            throw new CliException("The identity provider does not expose a device authorization endpoint.");
        }

        var clientId = context.ClientId ?? throw new CliException("The selected context does not declare a CLI client identifier.");
        var scope = string.Join(' ', context.Scopes.Count > 0 ? context.Scopes : ["openid", "profile", "offline_access"]);
        using var deviceRequest = new HttpRequestMessage(HttpMethod.Post, discovery.DeviceAuthorizationEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["scope"] = scope,
            }),
        };
        using var deviceResponse = await _httpClient.SendAsync(deviceRequest, cancellationToken);
        var deviceContent = await deviceResponse.Content.ReadAsStringAsync(cancellationToken);
        deviceResponse.EnsureSuccessStatusCode();

        var device = CliUtilities.ParseDeviceAuthorizationResponse(deviceContent);
        var verificationUri = CliUriPolicy.RequireSameOrigin(new Uri(discovery.Issuer), device.VerificationUriComplete ?? device.VerificationUri);
        var now = DateTimeOffset.UtcNow;
        var interval = Math.Max(1, device.Interval);
        return new DeviceLoginSession
        {
            ContextName = context.Name,
            TenantUrl = context.TenantUrl,
            Issuer = discovery.Issuer,
            ClientId = clientId,
            Scopes = [.. context.Scopes],
            TokenEndpoint = discovery.TokenEndpoint,
            DeviceCode = device.DeviceCode,
            UserCode = device.UserCode,
            VerificationUri = verificationUri.AbsoluteUri,
            ExpiresAt = now.AddSeconds(device.ExpiresIn),
            IntervalSeconds = interval,
            NextPollAt = now.AddSeconds(interval),
        };
    }

    public async Task<StoredToken> WaitForDeviceAuthorizationAsync(DeviceLoginSession session, CancellationToken cancellationToken,
        Func<CancellationToken, Task>? savePollingState = null)
    {
        CliUriPolicy.RequireSameOrigin(new Uri(session.Issuer), session.TokenEndpoint);
        while (DateTimeOffset.UtcNow < session.ExpiresAt)
        {
            var next = session.NextPollAt < session.ExpiresAt ? session.NextPollAt : session.ExpiresAt;
            var delay = next - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }
            cancellationToken.ThrowIfCancellationRequested();
            if (DateTimeOffset.UtcNow >= session.ExpiresAt)
            {
                break;
            }

            // Persist before the request, so a restart cannot poll faster than allowed.
            session.NextPollAt = DateTimeOffset.UtcNow.AddSeconds(session.IntervalSeconds);
            if (savePollingState is not null)
            {
                await savePollingState(cancellationToken);
            }
            using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, session.TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code",
                    ["client_id"] = session.ClientId,
                    ["device_code"] = session.DeviceCode,
                }),
            };
            using var tokenResponse = await _httpClient.SendAsync(tokenRequest, cancellationToken);
            var tokenContent = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
            if ((int)tokenResponse.StatusCode >= 500 || tokenResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                tokenResponse.EnsureSuccessStatusCode();
            }
            var parsed = CliUtilities.ParseTokenResponse(tokenContent);
            if (tokenResponse.IsSuccessStatusCode)
            {
                return CliUtilities.CreateStoredToken(parsed, new OidcDiscoveryDocument { Issuer = session.Issuer });
            }

            if (parsed.Error == "authorization_pending")
            {
                continue;
            }
            if (parsed.Error == "slow_down")
            {
                session.IntervalSeconds = (int)Math.Min((long)session.IntervalSeconds + 5, int.MaxValue);
                session.NextPollAt = DateTimeOffset.UtcNow.AddSeconds(session.IntervalSeconds);
                if (savePollingState is not null)
                {
                    // Keep the increased interval even if cancellation arrived
                    // with the response; resuming must still honor slow_down.
                    await savePollingState(CancellationToken.None);
                }
                continue;
            }
            throw new DeviceAuthorizationException(parsed.Error switch
            {
                "access_denied" => "The device authorization request was denied.",
                "expired_token" => "The device authorization code expired before sign-in completed.",
                _ => parsed.ErrorDescription ?? parsed.Error ?? "The device authorization flow failed.",
            });
        }

        throw new DeviceAuthorizationException("The device authorization code expired before sign-in completed. Run 'pomi login device start' again.");
    }

    public async Task<StoredToken> RefreshAsync(TenantContextRecord context, OidcDiscoveryDocument discovery, StoredToken storedToken, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(discovery);
        ArgumentNullException.ThrowIfNull(storedToken);

        if (string.IsNullOrWhiteSpace(storedToken.RefreshToken))
        {
            throw new CliException("The stored login does not contain a refresh token. Run 'pomi login' again.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, discovery.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = context.ClientId ?? throw new CliException("The selected context does not declare a CLI client identifier."),
                ["refresh_token"] = storedToken.RefreshToken,
            }),
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();
        var tokenResponse = CliUtilities.ParseTokenResponse(content);
        var refreshed = CliUtilities.CreateStoredToken(tokenResponse, discovery);
        refreshed.RefreshToken ??= storedToken.RefreshToken;
        return refreshed;
    }

    public async Task<StoredToken> ClientCredentialsAsync(string tokenEndpoint, string clientId, string clientSecret, string scope, string issuer, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenEndpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientSecret);
        CliUriPolicy.RequireSameOrigin(CliUriPolicy.RequireSecureEndpoint(issuer), tokenEndpoint);

        using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["scope"] = scope,
            }),
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();
        var tokenResponse = CliUtilities.ParseTokenResponse(content);
        return new StoredToken
        {
            AccessToken = tokenResponse.AccessToken ?? throw new CliException("The token response did not contain an access token."),
            TokenType = tokenResponse.TokenType,
            Scope = tokenResponse.Scope,
            Issuer = issuer,
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn > 0 ? tokenResponse.ExpiresIn : 3600),
        };
    }

    public async Task RevokeAsync(
        string revocationEndpoint,
        string clientId,
        string token,
        string tokenTypeHint,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, revocationEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["token"] = token,
                ["token_type_hint"] = tokenTypeHint,
            }),
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static string BuildAuthorizationUri(string endpoint, string clientId, string redirectUri, string scope, string state, string codeChallenge)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["client_id"] = clientId;
        query["response_type"] = "code";
        query["redirect_uri"] = redirectUri;
        query["scope"] = scope;
        query["state"] = state;
        query["code_challenge"] = codeChallenge;
        query["code_challenge_method"] = "S256";

        return $"{endpoint}?{query}";
    }

    private static string CreateRandomString()
    {
        var buffer = RandomNumberGenerator.GetBytes(32);
        return CliUtilities.Base64UrlEncode(buffer);
    }

    private static void LaunchBrowser(string url)
    {
        ProcessStartInfo startInfo;

        if (OperatingSystem.IsMacOS())
        {
            startInfo = new ProcessStartInfo("/usr/bin/open")
            {
                UseShellExecute = false,
            };
        }
        else if (OperatingSystem.IsWindows())
        {
            startInfo = new ProcessStartInfo(url)
            {
                UseShellExecute = true,
            };
        }
        else
        {
            startInfo = new ProcessStartInfo("xdg-open")
            {
                UseShellExecute = false,
            };
        }

        if (!OperatingSystem.IsWindows())
        {
            startInfo.ArgumentList.Add(url);
        }

        _ = Process.Start(startInfo) ?? throw new CliException("Failed to launch the system browser.");
    }

    private sealed class LoopbackCallbackListener : IAsyncDisposable
    {
        private readonly TcpListener _listener;

        private LoopbackCallbackListener(TcpListener listener)
        {
            _listener = listener;
            var endpoint = (IPEndPoint)listener.LocalEndpoint;
            RedirectUri = $"http://127.0.0.1:{endpoint.Port}/callback";
        }

        public string RedirectUri { get; }

        public static Task<LoopbackCallbackListener> StartAsync(CancellationToken cancellationToken)
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start(1);
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new LoopbackCallbackListener(listener));
        }

        public async Task<(string Code, string State)> WaitForCallbackAsync(string expectedState, string expectedIssuer, CancellationToken cancellationToken)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromMinutes(5));

            using var client = await _listener.AcceptTcpClientAsync(timeout.Token);
            await using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII, false, leaveOpen: true);
            using var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true)
            {
                NewLine = "\r\n",
                AutoFlush = true,
            };

            var requestLine = await ReadBoundedLineAsync(reader, timeout.Token) ?? throw new CliException("The browser callback was empty.");
            var requestTarget = requestLine.Split(' ', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1)
                ?? throw new CliException("The browser callback was malformed.");

            var headerLength = 0;
            string? header;
            while (!string.IsNullOrEmpty(header = await ReadBoundedLineAsync(reader, timeout.Token)))
            {
                headerLength += header.Length;
                if (headerLength > 16384)
                {
                    throw new CliException("The browser callback headers are too large.");
                }
            }

            var targetUri = new Uri($"http://127.0.0.1{requestTarget}", UriKind.Absolute);
            if (!requestLine.StartsWith("GET /callback?", StringComparison.Ordinal) || targetUri.AbsolutePath != "/callback")
            {
                throw new CliException("The browser callback did not target the expected route.");
            }

            var query = HttpUtility.ParseQueryString(targetUri.Query);
            if (!string.Equals(query["state"], expectedState, StringComparison.Ordinal))
            {
                throw new CliException("OAuth state validation failed.");
            }

            if (query["iss"] is { } issuer)
            {
                CliUtilities.EnsureIssuerMatches(expectedIssuer, issuer);
            }

            if (query["error"] is not null)
            {
                await OAuthCallbackPage.WriteAsync(writer, authorizationReceived: false);
                throw new CliException("The authorization request was denied or could not be completed. Run 'pomi login' to retry.");
            }

            var code = query["code"] ?? throw new CliException("The browser callback did not include an authorization code.");
            var state = query["state"] ?? throw new CliException("The browser callback did not include state.");

            await OAuthCallbackPage.WriteAsync(writer, authorizationReceived: true);
            return (code, state);
        }

        private static async Task<string?> ReadBoundedLineAsync(StreamReader reader, CancellationToken cancellationToken)
        {
            var builder = new StringBuilder();
            var character = new char[1];
            while (await reader.ReadAsync(character.AsMemory(), cancellationToken) > 0)
            {
                if (character[0] == '\n')
                {
                    return builder.ToString().TrimEnd('\r');
                }

                if (builder.Length >= 8192)
                {
                    throw new CliException("The browser callback line is too large.");
                }

                builder.Append(character[0]);
            }

            return builder.Length == 0 ? null : builder.ToString();
        }

        public ValueTask DisposeAsync()
        {
            _listener.Stop();
            return ValueTask.CompletedTask;
        }
    }
}
