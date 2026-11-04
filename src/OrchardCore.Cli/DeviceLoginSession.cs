using System.Text.Json.Nodes;

namespace OrchardCore.Cli;

internal sealed class DeviceLoginSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString("n");
    public string ContextName { get; set; } = string.Empty;
    public string TenantUrl { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = [];
    public string TokenEndpoint { get; set; } = string.Empty;
    public string DeviceCode { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public string VerificationUri { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public int IntervalSeconds { get; set; }
    public DateTimeOffset NextPollAt { get; set; }

    public JsonObject ToPublicJson(bool includeQr, bool includeSessionId = true)
    {
        var result = new JsonObject
        {
            ["status"] = "authorization_pending",
            ["context"] = ContextName,
            ["verificationUri"] = VerificationUri,
            ["userCode"] = UserCode,
            ["expiresAt"] = ExpiresAt,
        };
        if (includeSessionId)
        {
            result["sessionId"] = SessionId;
        }
        if (includeQr)
        {
            var png = TerminalQrCode.RenderPngBase64(VerificationUri);
            result["qrCode"] = png is null ? null : new JsonObject { ["mediaType"] = "image/png", ["base64"] = png };
        }
        return result;
    }
}

internal sealed class DeviceAuthorizationException : CliException
{
    public DeviceAuthorizationException(string message) : base(message)
    {
    }
}
