namespace OrchardCore.Cli;

internal static class OAuthCallbackPage
{
    private static readonly string _favicon = LoadFavicon();

    public static string Render(bool authorizationReceived)
    {
        var title = authorizationReceived ? "Authorization received" : "Authorization not completed";
        var status = authorizationReceived ? "Browser step complete" : "Login interrupted";
        var message = authorizationReceived
            ? "Your authorization has been sent to the Pomi CLI."
            : "Access was denied or the authorization request could not be completed.";
        var nextStep = authorizationReceived
            ? "Check your terminal to confirm that login completed."
            : "Run <code>pomi login</code> in your terminal to try again.";

        // CSS, system fonts, and inline SVG keep this page self-contained.
        // All interpolated content is fixed text. Never include callback parameters,
        // codes, tokens, or identity-provider error descriptions in this page.
        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <meta name="color-scheme" content="light dark">
              <title>{{title}} · Pomi CLI</title>
              <link rel="icon" type="image/png" sizes="32x32" href="data:image/png;base64,{{_favicon}}">
              <style>
                :root { color-scheme: light dark; --bg: #f5f8f6; --surface: #fff; --text: #0f1a14; --muted: #566058; --line: #e2e8e3; --accent: #15803d; --on-accent: #fff; --tint: #edf2ee; }
                * { box-sizing: border-box; }
                body { isolation: isolate; margin: 0; min-height: 100svh; padding: 48px 24px; display: grid; place-items: center; background: var(--bg); color: var(--text); font: 16px/1.6 system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif; }
                .background { position: fixed; inset: 0; z-index: -1; overflow: hidden; pointer-events: none; user-select: none; color: var(--accent); opacity: .05; }
                .background svg { position: absolute; right: -100px; bottom: -100px; width: clamp(360px, 65vw, 680px); height: auto; }
                .page { width: 100%; max-width: 560px; }
                .brand { display: flex; flex-wrap: wrap; align-items: center; gap: 12px; margin: 0 0 24px 4px; font-weight: 650; }
                .brand-mark { display: grid; place-items: center; width: 54px; height: 38px; border-radius: 11px; background: var(--accent); color: var(--on-accent); font: 600 18px ui-monospace, monospace; }
                .brand span:last-child { color: var(--muted); font-size: 13px; font-weight: 450; padding-left: 12px; border-left: 1px solid var(--line); }
                main { padding: 40px; border: 1px solid var(--line); border-radius: 20px; background: var(--surface); box-shadow: 0 12px 40px #0f1a140a; }
                .status { display: flex; align-items: center; gap: 8px; margin: 0 0 18px; color: var(--accent); font-size: 12px; font-weight: 700; letter-spacing: .08em; text-transform: uppercase; }
                .status::before { content: ""; width: 7px; height: 7px; border-radius: 50%; background: currentColor; }
                h1 { margin: 0 0 14px; font-size: clamp(26px, 5vw, 34px); font-family: Georgia, "Times New Roman", serif; font-weight: 600; line-height: 1.2; letter-spacing: -.035em; }
                .message { margin: 0; color: var(--muted); }
                .next { display: flex; gap: 14px; padding: 20px; margin-top: 30px; background: var(--tint); border: 1px solid var(--line); border-radius: 12px; }
                .next svg { flex: 0 0 24px; margin-top: 3px; color: var(--accent); }
                h2 { margin: 0 0 4px; font-size: 15px; font-weight: 650; }
                .next p { margin: 0; color: var(--muted); font-size: 14px; }
                code { font: .95em ui-monospace, monospace; color: var(--text); }
                footer { margin-top: 22px; text-align: center; color: var(--muted); font-size: 13px; }
                @media (max-width: 480px) { body { padding: 28px 16px; } main { padding: 28px 24px; } .brand { gap: 9px; } .next { padding: 16px; } }
                @media (prefers-color-scheme: dark) { :root { --bg: #07150d; --surface: #12291b; --text: #e4efe8; --muted: #9db2a4; --line: #21402e; --accent: #86cda0; --on-accent: #07150d; --tint: #16331f; } .background { opacity: .07; } }
              </style>
            </head>
            <body>
              <div class="background" aria-hidden="true">
                  <svg viewBox="68.84 223.2 148.88 148.88" fill="currentColor" focusable="false">
                      <path d="M143.28,223.2c-41.11,0-74.44,33.33-74.44,74.44s33.33,74.44,74.44,74.44c41.11,0,74.44-33.33,74.44-74.44S184.4,223.2,143.28,223.2z M143.28,357.75c-33.2,0-60.12-26.91-60.12-60.12c0-33.2,26.91-60.12,60.12-60.12c33.2,0,60.12,26.91,60.12,60.12C203.4,330.84,176.48,357.75,143.28,357.75z"/>
                      <path d="M179.12,333.16L179.12,333.16L179.12,333.16c-19.36-19.36-19.36-50.76,0-70.13l0,0l0,0C198.48,282.4,198.48,313.8,179.12,333.16z"/>
                      <path d="M159.2,313.24L159.2,313.24L159.2,313.24c-27.39,0-49.59-22.2-49.59-49.59v0h0C137,263.66,159.2,285.86,159.2,313.24z"/>
                      <path d="M108.77,333.35L108.77,333.35L108.77,333.35c19.36-19.36,50.76-19.36,70.13,0l0,0l0,0C159.53,352.71,128.13,352.71,108.77,333.35z"/>
                  </svg>
              </div>
              <div class="page">
                <header class="brand"><span class="brand-mark" aria-hidden="true">pomi</span> Orchard Core <span>Command-line interface</span></header>
                <main>
                  <p class="status">{{status}}</p>
                  <h1>{{title}}</h1>
                  <p class="message">{{message}}</p>
                  <section class="next" aria-labelledby="next-step">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="2" y="4" width="20" height="16" rx="3"/><path d="m6 9 3 3-3 3m7 0h4"/></svg>
                    <div><h2 id="next-step">Return to your terminal</h2><p>{{nextStep}}</p></div>
                  </section>
                </main>
                <footer>You can close this browser tab.</footer>
              </div>
            </body>
            </html>
            """;
    }

    public static Task WriteAsync(TextWriter writer, bool authorizationReceived) => writer.WriteAsync(
        "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nCache-Control: no-store\r\n"
        + "Content-Security-Policy: default-src 'none'; img-src data:; style-src 'unsafe-inline'; base-uri 'none'; frame-ancestors 'none'\r\n"
        + "Referrer-Policy: no-referrer\r\nX-Content-Type-Options: nosniff\r\nConnection: close\r\n\r\n"
        + Render(authorizationReceived));

    private static string LoadFavicon()
    {
        using var resource = typeof(OAuthCallbackPage).Assembly.GetManifestResourceStream("PomiFavicon")
            ?? throw new InvalidOperationException("The embedded Pomi favicon is missing.");
        using var buffer = new MemoryStream();
        resource.CopyTo(buffer);
        return Convert.ToBase64String(buffer.ToArray());
    }
}
