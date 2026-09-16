using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli;

internal sealed class LocalSiteInstallOptions
{
    public required string Directory { get; init; }
    public required string SiteName { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public string RecipeName { get; init; } = "SaaS";
    public string DatabaseProvider { get; init; } = "Sqlite";
    public string? TablePrefix { get; init; }
    public string? Schema { get; init; }
    public string SiteTimeZone { get; init; } = "UTC";
    public string? RequestUrlPrefix { get; init; }
    public string? RequestUrlHost { get; init; }
    public string? Source { get; init; }
    public bool ClearSources { get; init; }
    public string? Urls { get; set; }
    public int SetupTimeoutSeconds { get; init; } = 300;
    public bool Verbose { get; init; }
    public RemoteManagementClientCredentials? ClientCredentials { get; init; }
    public string Password { get; set; } = string.Empty;
    public string? ConnectionString { get; set; }
    public string[] SecretEnvironmentVariables { get; init; } = [];
}

internal sealed class LocalSiteInstallOutput
{
    public string? Context { get; set; }

    public string Directory { get; init; } = string.Empty;
    public string Project { get; init; } = string.Empty;
    public string PackageVersion { get; init; } = string.Empty;
    public string SdkVersion { get; init; } = string.Empty;
    public string Tenant { get; init; } = "Default";
    public string TenantState { get; init; } = "Running";
    public string Url { get; init; } = string.Empty;
    public string ListenUrl { get; init; } = string.Empty;
}

internal static partial class LocalSiteInstaller
{
    internal const string NugetSource = "https://api.nuget.org/v3/index.json";
    private const string AutoSetupPrefix = "OrchardCore__OrchardCore_AutoSetup__";

    public static void Validate(LocalSiteInstallOptions options)
    {
        var directory = new DirectoryInfo(options.Directory);
        if (File.Exists(options.Directory) || directory.LinkTarget is not null || directory.Exists && directory.EnumerateFileSystemInfos().Any())
        {
            throw new CliException("Choose a new or empty directory. 'pomi install' does not overwrite an existing site or follow a destination symlink.");
        }

        if (string.IsNullOrWhiteSpace(options.SiteName) || string.IsNullOrWhiteSpace(options.UserName)
            || string.IsNullOrWhiteSpace(options.RecipeName) || !MailAddress.TryCreate(options.Email, out var email) || email.Address != options.Email)
        {
            throw new CliException("Provide a site name, user name, valid email address, and setup recipe name.");
        }

        if (string.IsNullOrWhiteSpace(options.DatabaseProvider) || string.IsNullOrWhiteSpace(options.SiteTimeZone))
        {
            throw new CliException("Provide a database provider and site time zone.");
        }

        if (options.SetupTimeoutSeconds is < 1 or > 3600)
        {
            throw new CliException("--setup-timeout must be between 1 and 3600 seconds.");
        }

        if (!string.IsNullOrEmpty(options.RequestUrlPrefix) && !UrlPrefixPattern().IsMatch(options.RequestUrlPrefix))
        {
            throw new CliException("--request-url-prefix must contain path segments made of letters, digits, underscores, or hyphens, without leading or trailing slashes.");
        }

        if (!string.IsNullOrEmpty(options.RequestUrlHost) && Uri.CheckHostName(options.RequestUrlHost) == UriHostNameType.Unknown)
        {
            throw new CliException("--request-url-host must be a single host name without a scheme, port, or path.");
        }

        if (options.Urls is not null)
        {
            _ = ParseListenUrls(options.Urls!);
        }
        _ = ResolveSource(options);
    }

    public static void ValidateSecrets(LocalSiteInstallOptions options)
    {
        SetupPasswordValidator.Validate(options.Password);

        if (!string.Equals(options.DatabaseProvider, "Sqlite", StringComparison.Ordinal) && string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new CliException("Provide --connection-string-env, --connection-string-file, or --connection-string-stdin for this database provider.");
        }
    }

    internal static Uri[] ParseListenUrls(string urls)
    {
        return urls.Split(';', StringSplitOptions.TrimEntries).Select(value =>
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")
                || uri.Port == 0 || uri.AbsolutePath != "/" || !string.IsNullOrEmpty(uri.UserInfo + uri.Query + uri.Fragment))
            {
                throw new CliException("--urls must be a quoted, semicolon-separated list of HTTP or HTTPS listen URLs without credentials, paths, queries, fragments, or empty entries. Example: --urls \"https://localhost:5001;http://localhost:5000\".");
            }

            return uri;
        }).ToArray();
    }

    internal static string ResolveSource(LocalSiteInstallOptions options)
    {
        var source = options.Source ?? NugetSource;
        if (Directory.Exists(source))
        {
            return Path.GetFullPath(source);
        }

        if (!Uri.TryCreate(source, UriKind.Absolute, out var uri) || uri.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo + uri.Query + uri.Fragment))
        {
            throw new CliException("--source must be an HTTPS NuGet feed URL without credentials, query, or fragment, or an existing local package directory.");
        }

        return source;
    }

    internal static string GetSiteUrl(LocalSiteInstallOptions options)
    {
        var urls = ParseListenUrls(options.Urls!);
        var uri = new UriBuilder(urls.FirstOrDefault(url => url.Scheme == "https") ?? urls[0])
        {
            Path = options.RequestUrlPrefix ?? string.Empty,
        };
        if (!string.IsNullOrEmpty(options.RequestUrlHost))
        {
            uri.Host = options.RequestUrlHost;
        }

        return uri.Uri.AbsoluteUri;
    }

    internal static async Task ExtractTemplateAsync(string destination, CancellationToken cancellationToken)
    {
        var assembly = typeof(Program).Assembly;
        foreach (var name in assembly.GetManifestResourceNames().Where(name => name.StartsWith("OccmsTemplate/", StringComparison.Ordinal)))
        {
            var relative = name["OccmsTemplate/".Length..].Replace('\\', '/').Replace(".template.config.src/", ".template.config/", StringComparison.Ordinal);
            var path = Path.Combine(destination, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await using var input = assembly.GetManifestResourceStream(name)!;
            await using var output = new FileStream(path, FileMode.CreateNew);
            await input.CopyToAsync(output, cancellationToken);
        }

        var configPath = Path.Combine(destination, ".template.config", "template.json");
        if (!File.Exists(configPath))
        {
            throw new CliException("The embedded CMS template is missing. Reinstall this CLI build.");
        }

        var config = await File.ReadAllTextAsync(configPath, cancellationToken);
        config = config.Replace("${TemplateOrchardVersion}", DotnetEnvironment.PackageVersion, StringComparison.Ordinal)
            .Replace("${TemplateTargetFramework}", $"net{DotnetEnvironment.RequiredMajor}.0", StringComparison.Ordinal);
        await File.WriteAllTextAsync(configPath, config, cancellationToken);
    }

    public static async Task<LocalSiteInstallOutput> InstallAsync(LocalSiteInstallOptions options, string sdk, TextWriter log, CancellationToken cancellationToken)
    {
        Validate(options);
        ValidateSecrets(options);
        using var ports = InstallPortReservation.Create(options.Urls);
        options.Urls = ports.Urls;
        using var diagnostics = new InstallProcessLog(log, options.Verbose);
        var completed = false;
        var scratch = Path.Combine(Path.GetTempPath(), "pomi-install-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(scratch);
        var environment = new Dictionary<string, string>
        {
            ["DOTNET_CLI_HOME"] = Path.Combine(scratch, "dotnet-home"),
            ["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1",
        };
        try
        {
            await log.WriteLineAsync($"Creating CMS {DotnetEnvironment.PackageVersion} from the embedded template using .NET SDK {sdk}.");
            var template = Path.Combine(scratch, "template");
            await ExtractTemplateAsync(template, cancellationToken);
            await WriteSdkSelectionAsync(scratch, sdk, cancellationToken);
            await DotnetEnvironment.RunAsync(["new", "install", template], scratch, environment, diagnostics, cancellationToken, options.SecretEnvironmentVariables);
            // Recheck after template extraction, before writing any project files.
            Validate(options);
            await DotnetEnvironment.RunAsync(["new", "occms", "--output", options.Directory], scratch, environment, diagnostics, cancellationToken, options.SecretEnvironmentVariables);
            await WriteSdkSelectionAsync(options.Directory, sdk, cancellationToken);
            await WriteListenUrlsAsync(options.Directory, options.Urls, cancellationToken);
            var sources = new XElement("packageSources");
            if (options.ClearSources)
            {
                sources.Add(new XElement("clear"));
            }

            sources.Add(new XElement("add", new XAttribute("key", "nuget.org"), new XAttribute("value", NugetSource)));
            var config = new XDocument(new XElement("configuration", sources));
            var source = ResolveSource(options);
            if (source != NugetSource)
            {
                sources.Add(new XElement("add", new XAttribute("key", "OrchardCore"), new XAttribute("value", source)));
            }

            var nugetConfig = Path.Combine(options.Directory, "NuGet.Config");
            await File.WriteAllTextAsync(nugetConfig, config.ToString(), cancellationToken);
            var programPath = Path.Combine(options.Directory, "Program.cs");
            var program = await File.ReadAllTextAsync(programPath, cancellationToken);
            const string orchardBuilder = ".AddOrchardCms()";
            if (program.Split(orchardBuilder).Length != 2)
            {
                throw new CliException("The embedded CMS template has an unsupported startup structure.");
            }

            await File.WriteAllTextAsync(programPath, program.Replace(orchardBuilder,
                orchardBuilder + global::System.Environment.NewLine + "    .AddSetupFeatures(\"OrchardCore.AutoSetup\")", StringComparison.Ordinal), cancellationToken);
            var project = Directory.GetFiles(options.Directory, "*.csproj").Single();
            await log.WriteLineAsync("Restoring and building the new site.");
            // Template installation uses an isolated CLI home. Restore/build must use
            // the caller's normal NuGet hierarchy, credentials, and CLI home instead.
            environment.Remove("DOTNET_CLI_HOME");
            await DotnetEnvironment.RunAsync(["restore", project, "--disable-build-servers"], options.Directory, environment, diagnostics, cancellationToken, options.SecretEnvironmentVariables);
            await DotnetEnvironment.RunAsync(["build", project, "--no-restore", "--disable-build-servers", "-m:1"], options.Directory, environment, diagnostics, cancellationToken, options.SecretEnvironmentVariables);
            await log.WriteLineAsync("Initializing the Default tenant.");
            await SetupAsync(options, project, diagnostics, cancellationToken);
            var listenUrls = ParseListenUrls(options.Urls!);
            var listenUrl = string.Join(';', listenUrls.Select(url => url.GetLeftPart(UriPartial.Authority)));
            completed = true;
            return new LocalSiteInstallOutput
            {
                Directory = options.Directory,
                Project = project,
                PackageVersion = DotnetEnvironment.PackageVersion,
                SdkVersion = sdk,
                Url = GetSiteUrl(options),
                ListenUrl = listenUrl,
            };
        }
        finally
        {
            if (!completed)
            {
                await diagnostics.WriteFailureAsync();
            }

            options.Password = string.Empty;
            options.ConnectionString = null;
            try
            {
                Directory.Delete(scratch, recursive: true);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                await log.WriteLineAsync($"Warning: could not remove temporary template files in '{scratch}'.");
            }
        }
    }

    internal static async Task WriteListenUrlsAsync(string directory, string urls, CancellationToken cancellationToken)
    {
        var path = Path.Combine(directory, "appsettings.json");
        var settings = JsonNode.Parse(await File.ReadAllTextAsync(path, cancellationToken), documentOptions: new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        })!.AsObject();
        settings["Urls"] = urls;
        await File.WriteAllTextAsync(path, settings.ToJsonString(new JsonSerializerOptions { WriteIndented = true, TypeInfoResolver = CliJsonContext.Default }), cancellationToken);

        // dotnet run's project profiles otherwise override appsettings with template ports.
        path = Path.Combine(directory, "Properties", "launchSettings.json");
        if (File.Exists(path))
        {
            var launch = JsonNode.Parse(await File.ReadAllTextAsync(path, cancellationToken), documentOptions: new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            })!.AsObject();
            if (launch["profiles"] is JsonObject profiles)
            {
                foreach (var profile in profiles.Select(entry => entry.Value).OfType<JsonObject>())
                {
                    if (profile["commandName"]?.GetValue<string>() == "Project")
                    {
                        profile["applicationUrl"] = urls;
                    }
                }
            }

            await File.WriteAllTextAsync(path, launch.ToJsonString(new JsonSerializerOptions { WriteIndented = true, TypeInfoResolver = CliJsonContext.Default }), cancellationToken);
        }
    }

    private static Task WriteSdkSelectionAsync(string directory, string sdk, CancellationToken cancellationToken) => File.WriteAllTextAsync(
        Path.Combine(directory, "global.json"), new JsonObject
        {
            ["sdk"] = new JsonObject { ["version"] = sdk, ["rollForward"] = "latestPatch", ["allowPrerelease"] = false },
        }.ToJsonString(), cancellationToken);

    internal static Dictionary<string, string> CreateSetupEnvironment(LocalSiteInstallOptions options, string setupPath)
    {
        var values = new Dictionary<string, string>
        {
            ["ShellName"] = "Default", ["SiteName"] = options.SiteName, ["SiteTimeZone"] = options.SiteTimeZone,
            ["AdminUsername"] = options.UserName, ["AdminEmail"] = options.Email, ["AdminPassword"] = options.Password,
            ["RecipeName"] = options.RecipeName, ["DatabaseProvider"] = options.DatabaseProvider,
            ["DatabaseConnectionString"] = options.ConnectionString ?? string.Empty,
            ["DatabaseTablePrefix"] = options.TablePrefix ?? string.Empty, ["DatabaseSchema"] = options.Schema ?? string.Empty,
            ["RequestUrlPrefix"] = options.RequestUrlPrefix ?? string.Empty, ["RequestUrlHost"] = options.RequestUrlHost ?? string.Empty,
        };
        if (options.ClientCredentials is { } credentials)
        {
            values["RemoteManagementClientId"] = credentials.ClientId;
            values["RemoteManagementClientSecret"] = credentials.ClientSecret;
        }

        var environment = values.ToDictionary(pair => AutoSetupPrefix + "Tenants__0__" + pair.Key, pair => pair.Value);
        environment[AutoSetupPrefix + "AutoSetupPath"] = setupPath;
        return environment;
    }

    private static async Task SetupAsync(LocalSiteInstallOptions options, string project, TextWriter log, CancellationToken cancellationToken)
    {
        var appData = Path.Combine(options.Directory, "App_Data");
        Directory.CreateDirectory(appData);
        CliPaths.SetOwnerOnlyDirectory(appData);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(options.SetupTimeoutSeconds));
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        var url = $"http://127.0.0.1:{port}";
        var setupPath = "/pomi-setup-" + Guid.NewGuid().ToString("N");
        await log.WriteLineAsync("Auto Setup uses a temporary local-only server; it stops before the requested site URL starts.");
        using var process = DotnetEnvironment.Start([ApplicationPath(project), "--urls", url], options.Directory,
            CreateSetupEnvironment(options, setupPath), options.SecretEnvironmentVariables);
        string[] secrets = [options.Password, options.ConnectionString ?? string.Empty, options.ClientCredentials?.ClientSecret ?? string.Empty];
        var stdout = DotnetEnvironment.DrainAsync(process.StandardOutput, log, secrets: secrets);
        var stderr = DotnetEnvironment.DrainAsync(process.StandardError, log, secrets: secrets);
        try
        {
            using var handler = new SocketsHttpHandler { AllowAutoRedirect = false, UseProxy = false };
            using var client = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
            while (true)
            {
                timeout.Token.ThrowIfCancellationRequested();
                if (process.HasExited)
                {
                    throw new CliException("The setup host exited before setup completed. See the application output above.");
                }

                using var request = new HttpRequestMessage(HttpMethod.Get, url + setupPath);
                if (!string.IsNullOrEmpty(options.RequestUrlHost))
                {
                    request.Headers.Host = options.RequestUrlHost;
                }

                try
                {
                    using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
                    if (response.StatusCode != HttpStatusCode.Redirect || !await IsInitializedAsync(options.Directory, timeout.Token))
                    {
                        throw new CliException($"Auto Setup did not complete (HTTP {(int)response.StatusCode}). Check the recipe, database settings, and password requirements in the application output above. The project is preserved.");
                    }

                    if (options.ClientCredentials is { } credentials &&
                        (!response.Headers.TryGetValues("X-OrchardCore-Provisioned-Client", out var provisioned) ||
                         !provisioned.Contains(credentials.ClientId, StringComparer.Ordinal)))
                    {
                        throw new CliException("The site was installed, but its Auto Setup module did not confirm application provisioning. Use matching server packages that support --enable-remote-management. The project is preserved.");
                    }

                    return;
                }
                catch (HttpRequestException)
                {
                    await Task.Delay(250, timeout.Token);
                }
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new CliException($"Auto Setup exceeded {options.SetupTimeoutSeconds} seconds. The temporary host was stopped and the project is preserved.");
        }
        finally
        {
            await DotnetEnvironment.StopAsync(process);
            await Task.WhenAll(stdout, stderr);
        }
    }

    internal static async Task<bool> IsInitializedAsync(string directory, CancellationToken cancellationToken)
    {
        var path = Path.Combine(directory, "App_Data", "tenants.json");
        if (!File.Exists(path))
        {
            return false;
        }

        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(path, cancellationToken));
        return document.RootElement.TryGetProperty("Default", out var tenant)
            && tenant.TryGetProperty("State", out var state) && state.GetString() == "Running";
    }

    private static string ApplicationPath(string project) => Path.Combine(Path.GetDirectoryName(project)!, "bin", "Debug",
        $"net{DotnetEnvironment.RequiredMajor}.0", Path.GetFileNameWithoutExtension(project) + ".dll");

    public static async Task RunAsync(LocalSiteInstallOutput output, TextWriter log, CancellationToken cancellationToken,
        IEnumerable<string>? secretEnvironmentVariables = null)
    {
        // The installation reservation must be released before the child server can bind.
        // Check again to report a useful error if another process took a port meanwhile.
        using (InstallPortReservation.Create(output.ListenUrl))
        {
        }

        await log.WriteLineAsync($"Starting {output.Url} — press Ctrl+C to stop.");
        await DotnetEnvironment.RunAsync([ApplicationPath(output.Project), "--urls", output.ListenUrl], output.Directory, null, log, cancellationToken, secretEnvironmentVariables);
    }

    [GeneratedRegex("^[a-zA-Z0-9_-]+(/[a-zA-Z0-9_-]+)*$")]
    private static partial Regex UrlPrefixPattern();
}
