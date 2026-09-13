using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli.Tests;

public class LocalSiteInstallerTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SetupEnvironment_ApplicationProvisioning_IsOptInAndUsesOnlyEnvironment(bool enable)
    {
        var credentials = enable ? RemoteManagementClientCredentials.Generate() : null;
        var options = new LocalSiteInstallOptions
        {
            Directory = "unused", SiteName = "Test", UserName = "admin", Email = "admin@example.com",
            ClientCredentials = credentials,
        };
        var environment = LocalSiteInstaller.CreateSetupEnvironment(options, "/setup");
        const string prefix = "OrchardCore__OrchardCore_AutoSetup__Tenants__0__RemoteManagement";
        Assert.Equal(enable, environment.ContainsKey(prefix + "ClientId"));
        Assert.Equal(enable, environment.ContainsKey(prefix + "ClientSecret"));
        var process = DotnetEnvironment.CreateStartInfo(["site.dll"], "unused", environment);
        Assert.DoesNotContain(process.ArgumentList, argument => argument.Contains("pomi-", StringComparison.Ordinal));
        if (enable)
        {
            Assert.Equal(credentials!.ClientSecret, process.Environment[prefix + "ClientSecret"]);
        }
    }

    [Fact]
    public void InstallDefaults_UseNugetOrgAndHttps()
    {
        var options = CreateOptions();
        Assert.Equal("https://api.nuget.org/v3/index.json", LocalSiteInstaller.ResolveSource(options));
        using var ports = InstallPortReservation.Create(options.Urls);
        options.Urls = ports.Urls;
        var url = new Uri(LocalSiteInstaller.GetSiteUrl(options));
        Assert.Equal("https", url.Scheme);
        Assert.Equal("localhost", url.Host);
        Assert.InRange(url.Port, 1, 65535);
    }

    [Fact]
    public void AutomaticPorts_StayReservedAndConcurrentInstallsChooseDifferentPorts()
    {
        using var first = InstallPortReservation.Create(null);
        using var second = InstallPortReservation.Create(null);
        Assert.NotEqual(first.Urls, second.Urls);
        var error = Assert.Throws<CliException>(() => InstallPortReservation.Create(first.Urls));
        Assert.Contains("--urls", error.Message);
        Assert.Contains("already in use", error.Message);
        first.Dispose();
        using var released = InstallPortReservation.Create(first.Urls);
        Assert.Equal(first.Urls, released.Urls);
    }

    [Fact]
    public void ExplicitPorts_CheckEveryAddressAndReleaseEarlierReservationsOnFailure()
    {
        using var available = InstallPortReservation.Create(null);
        var firstUrl = available.Urls;
        available.Dispose();
        using var busy = InstallPortReservation.Create(null);
        Assert.Throws<CliException>(() => InstallPortReservation.Create(firstUrl + ";" + busy.Urls));
        using var recovered = InstallPortReservation.Create(firstUrl);
        Assert.Equal(firstUrl, recovered.Urls);
    }

    [Fact]
    public void LocalhostPorts_RejectAnOccupiedIpv6Address()
    {
        if (!Socket.OSSupportsIPv6)
        {
            return;
        }

        using var busy = new TcpListener(IPAddress.IPv6Loopback, 0);
        busy.Server.DualMode = false;
        busy.Start();
        var port = ((IPEndPoint)busy.LocalEndpoint).Port;
        Assert.Throws<CliException>(() => InstallPortReservation.Create($"https://localhost:{port}"));
    }

    [Fact]
    public void Ipv6Wildcard_RejectsOccupiedIpv4WildcardPort()
    {
        using var busy = new TcpListener(IPAddress.Any, 0);
        busy.Start();
        var port = ((IPEndPoint)busy.LocalEndpoint).Port;
        Assert.Throws<CliException>(() => InstallPortReservation.Create($"http://[::]:{port}"));
    }

    [Fact]
    public async Task OccupiedExplicitPort_FailsBeforeSdkOrPasswordAndDoesNotCreateProject()
    {
        using var busy = InstallPortReservation.Create(null);
        var directory = TestPaths.CreateScratchDirectory(nameof(OccupiedExplicitPort_FailsBeforeSdkOrPasswordAndDoesNotCreateProject));
        var args = new[] { "install", directory, "--site-name", "Test", "--email", "admin@example.com", "--urls", busy.Urls };
        using var client = new HttpClient(new RejectNetworkHandler());
        var app = await CliApplication.CreateAsync(args, new CliPaths(TestPaths.CreateScratchDirectory("port-test-config")), client, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());
        using var errors = new StringWriter();
        Assert.Equal(1, await app.InvokeAsync(args, errors));
        Assert.Contains("already in use", errors.ToString());
        Assert.Empty(Directory.GetFiles(directory, "*.csproj"));
    }

    [Fact]
    public async Task PersistedUrls_UpdateProjectProfilesAndPreserveOtherSettings()
    {
        var directory = TestPaths.CreateScratchDirectory(nameof(PersistedUrls_UpdateProjectProfilesAndPreserveOtherSettings));
        Directory.CreateDirectory(Path.Combine(directory, "Properties"));
        await File.WriteAllTextAsync(Path.Combine(directory, "appsettings.json"), """{"AllowedHosts":"*",/* template comment */"Logging":{"Level":"Warning"}}""", TestContext.Current.CancellationToken);
        var launchPath = Path.Combine(directory, "Properties", "launchSettings.json");
        await File.WriteAllTextAsync(launchPath, """{"profiles":{"Cms":{"commandName":"Project","applicationUrl":"https://localhost:5001","environmentVariables":{"ASPNETCORE_ENVIRONMENT":"Development"}},"IIS":{"commandName":"IISExpress"}}}""", TestContext.Current.CancellationToken);
        const string urls = "https://localhost:53127;http://localhost:53128";
        await LocalSiteInstaller.WriteListenUrlsAsync(directory, urls, TestContext.Current.CancellationToken);
        using var settings = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(directory, "appsettings.json"), TestContext.Current.CancellationToken));
        Assert.Equal(urls, settings.RootElement.GetProperty("Urls").GetString());
        Assert.Equal("Warning", settings.RootElement.GetProperty("Logging").GetProperty("Level").GetString());
        using var launch = JsonDocument.Parse(await File.ReadAllTextAsync(launchPath, TestContext.Current.CancellationToken));
        var profiles = launch.RootElement.GetProperty("profiles");
        Assert.Equal(urls, profiles.GetProperty("Cms").GetProperty("applicationUrl").GetString());
        Assert.Equal("Development", profiles.GetProperty("Cms").GetProperty("environmentVariables").GetProperty("ASPNETCORE_ENVIRONMENT").GetString());
        Assert.False(profiles.GetProperty("IIS").TryGetProperty("applicationUrl", out _));
    }

    [Theory]
    [InlineData("https://packages.example/v3/index.json")]
    [InlineData("https://nuget.cloudsmith.io/orchardcore/preview/v3/index.json")]
    public void Source_UsesExplicitFeed(string source)
    {
        Assert.Equal(source, LocalSiteInstaller.ResolveSource(new LocalSiteInstallOptions
        {
            Directory = "unused", SiteName = "Test", UserName = "admin", Email = "admin@example.com", Source = source,
        }));
    }

    [Theory]
    [InlineData("http://localhost:5000;https://localhost:5001", "https://site.example:5001/news")]
    [InlineData(" http://localhost:5000 ; http://localhost:5080 ", "http://site.example:5000/news")]
    [InlineData("https://[::1]:5001;http://127.0.0.1:5000", "https://site.example:5001/news")]
    public void ListenUrls_AcceptMultipleAddressesAndPreferHttps(string urls, string expected)
    {
        Assert.Equal(2, LocalSiteInstaller.ParseListenUrls(urls).Length);
        Assert.Equal(expected, LocalSiteInstaller.GetSiteUrl(new LocalSiteInstallOptions
        {
            Directory = "unused", SiteName = "Test", UserName = "admin", Email = "admin@example.com",
            Urls = urls, RequestUrlHost = "site.example", RequestUrlPrefix = "news",
        }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("https://localhost:5001;")]
    [InlineData("https://localhost:5001;;http://localhost:5000")]
    [InlineData("https://localhost:5001;http://localhost:0")]
    [InlineData("https://localhost:5001;ftp://localhost:5000")]
    [InlineData("https://localhost:5001;http://user:password@localhost:5000")]
    [InlineData("https://localhost:5001;http://localhost:5000/path")]
    [InlineData("https://localhost:5001;http://localhost:5000?query=value")]
    [InlineData("https://localhost:5001;http://localhost:5000#fragment")]
    public void ListenUrls_ValidateEveryAddress(string urls)
    {
        Assert.Throws<CliException>(() => LocalSiteInstaller.ParseListenUrls(urls));
    }

    [Fact]
    public void SelectSdk_RequiresStableMatchingMajorAndChoosesNewestPatch()
    {
        var major = DotnetEnvironment.RequiredMajor;
        Assert.Equal($"{major}.0.302", DotnetEnvironment.SelectSdk($"""
            {major - 1}.0.999 [/sdk]
            {major}.0.100 [/sdk]
            {major}.0.302 [/sdk]
            {major}.0.400-preview.1 [/sdk]
            {major + 1}.0.100 [/sdk]
            """));
        Assert.Null(DotnetEnvironment.SelectSdk($"{major - 1}.0.999 [/sdk]\n{major}.0.100-preview.1 [/sdk]"));
        Assert.Null(DotnetEnvironment.SelectSdk(string.Empty));
    }

    [Fact]
    public async Task EmbeddedTemplate_ContainsOnlyCmsAndUsesThisBuildVersion()
    {
        var directory = TestPaths.CreateScratchDirectory(nameof(EmbeddedTemplate_ContainsOnlyCmsAndUsesThisBuildVersion));
        await LocalSiteInstaller.ExtractTemplateAsync(directory, TestContext.Current.CancellationToken);
        using var config = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(directory, ".template.config", "template.json"), TestContext.Current.CancellationToken));
        Assert.Equal("occms", config.RootElement.GetProperty("shortName").GetString());
        Assert.Equal(DotnetEnvironment.PackageVersion, config.RootElement.GetProperty("symbols").GetProperty("OrchardVersion").GetProperty("defaultValue").GetString());
        Assert.Equal($"net{DotnetEnvironment.RequiredMajor}.0", config.RootElement.GetProperty("symbols").GetProperty("Framework").GetProperty("defaultValue").GetString());
        Assert.Single(Directory.GetFiles(directory, "*.csproj", SearchOption.AllDirectories));
        Assert.True(File.Exists(Path.Combine(directory, "Program.cs")));
        Assert.True(File.Exists(Path.Combine(directory, "NLog.config")));
        Assert.False(Directory.Exists(Path.Combine(directory, ".template.config.src")));
    }

    [Fact]
    public void Validate_RefusesExistingContentWithoutChangingIt()
    {
        var options = CreateOptions();
        var existing = Path.Combine(options.Directory, "keep.txt");
        File.WriteAllText(existing, "keep me");
        Assert.Throws<CliException>(() => LocalSiteInstaller.Validate(options));
        Assert.Equal("keep me", File.ReadAllText(existing));
        Assert.Single(Directory.GetFiles(options.Directory));
    }

    [Fact]
    public void Validate_RefusesDestinationSymlink()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var directory = TestPaths.CreateScratchDirectory(nameof(Validate_RefusesDestinationSymlink));
        var target = Path.Combine(directory, "target");
        Directory.CreateDirectory(target);
        var link = Path.Combine(directory, "link");
        Directory.CreateSymbolicLink(link, target);
        Assert.Throws<CliException>(() => LocalSiteInstaller.Validate(CreateOptions(link)));
        Assert.Empty(Directory.GetFileSystemEntries(target));
    }

    [Fact]
    public void SetupProcess_UsesEnvironmentForSecretsAndArgumentListForPaths()
    {
        var options = CreateOptions();
        options.Password = "not-on-command-line";
        options.ConnectionString = "Server=localhost;Password=not-on-command-line";
        var environment = LocalSiteInstaller.CreateSetupEnvironment(options, "/random-setup");
        var info = DotnetEnvironment.CreateStartInfo(["path with spaces/site.dll", "--urls", "http://127.0.0.1:12345"], options.Directory, environment);
        Assert.False(info.UseShellExecute);
        Assert.Equal("path with spaces/site.dll", info.ArgumentList[0]);
        Assert.DoesNotContain(options.Password, string.Join(' ', info.ArgumentList));
        Assert.Equal(options.Password, info.Environment["OrchardCore__OrchardCore_AutoSetup__Tenants__0__AdminPassword"]);
        Assert.Equal(options.ConnectionString, info.Environment["OrchardCore__OrchardCore_AutoSetup__Tenants__0__DatabaseConnectionString"]);
        Assert.Equal("Default", info.Environment["OrchardCore__OrchardCore_AutoSetup__Tenants__0__ShellName"]);
        var normalRun = DotnetEnvironment.CreateStartInfo(["site.dll"], options.Directory, null);
        Assert.DoesNotContain(normalRun.Environment.Keys, key => key.StartsWith("OrchardCore__OrchardCore_AutoSetup__", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(Directory.GetFileSystemEntries(options.Directory));
    }

    [Theory]
    [InlineData("Uninitialized", false)]
    [InlineData("Initializing", false)]
    [InlineData("Running", true)]
    [InlineData("Disabled", false)]
    public async Task SetupCompletion_RequiresPersistedRunningDefaultTenant(string state, bool expected)
    {
        var directory = TestPaths.CreateScratchDirectory(nameof(SetupCompletion_RequiresPersistedRunningDefaultTenant));
        Assert.False(await LocalSiteInstaller.IsInitializedAsync(directory, TestContext.Current.CancellationToken));
        Directory.CreateDirectory(Path.Combine(directory, "App_Data"));
        await File.WriteAllTextAsync(Path.Combine(directory, "App_Data", "tenants.json"), $$$"""{"Default":{"State":"{{{state}}}"}}""", TestContext.Current.CancellationToken);
        Assert.Equal(expected, await LocalSiteInstaller.IsInitializedAsync(directory, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ProcessLog_RedactsSecrets()
    {
        await using var input = new MemoryStream("password=a-secret\nconnection=another-secret"u8.ToArray());
        using var reader = new StreamReader(input);
        using var output = new StringWriter();
        await DotnetEnvironment.DrainAsync(reader, output, secrets: ["a-secret", "another-secret"]);
        Assert.DoesNotContain("a-secret", output.ToString());
        Assert.DoesNotContain("another-secret", output.ToString());
        Assert.Contains("[redacted]", output.ToString());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InstallLog_ShowsDiagnosticsOnFailureOrWhenVerbose(bool verbose)
    {
        using var output = new StringWriter();
        using var log = new InstallProcessLog(output, verbose);
        using var reader = new StreamReader(new MemoryStream("setup failed: a-secret"u8.ToArray()));
        await DotnetEnvironment.DrainAsync(reader, log, secrets: ["a-secret"]);
        Assert.Equal(verbose, output.ToString().Contains("setup failed", StringComparison.Ordinal));
        await log.WriteFailureAsync();
        Assert.Contains("setup failed: [redacted]", output.ToString());
        Assert.DoesNotContain("a-secret", output.ToString());
        Assert.Equal(1, output.ToString().Split("setup failed").Length - 1);
    }

    [Fact]
    public async Task InstallLog_BoundsDiagnosticsAndRetainsLatestFailure()
    {
        using var output = new StringWriter();
        using var log = new InstallProcessLog(output, verbose: false);
        await log.WriteLineAsync("old output");
        await log.WriteLineAsync(new string('x', InstallProcessLog.Capacity * 2));
        await log.WriteLineAsync("latest failure");
        Assert.Empty(output.ToString());
        await log.WriteFailureAsync();
        Assert.DoesNotContain("old output", output.ToString());
        Assert.Contains("earlier output omitted", output.ToString());
        Assert.Contains("latest failure", output.ToString());
        Assert.True(output.ToString().Length < InstallProcessLog.Capacity + 200);
    }

    [Fact]
    public async Task InstallHelp_IsLocalAndDoesNotOfferInlineSecrets()
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(InstallHelp_IsLocalAndDoesNotOfferInlineSecrets)));
        using var client = new HttpClient(new RejectNetworkHandler());
        await new ContextStore(paths).SaveAsync(new CliConfiguration
        {
            CurrentContext = "offline",
            Contexts = [new TenantContextRecord { Name = "offline", TenantUrl = "https://offline.example/" }],
        }, TestContext.Current.CancellationToken);
        var args = new[] { "install", "--help" };
        var app = await CliApplication.CreateAsync(args, paths, client, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());
        var install = Assert.Single(app.RootCommand.Subcommands, command => command.Name == "install");
        Assert.Contains(install.Options, option => option.Name == "--password-env");
        Assert.Contains(install.Options, option => option.Name == "--password-file");
        Assert.Contains(install.Options, option => option.Name == "--password-stdin");
        Assert.DoesNotContain(install.Options, option => option.Name is "--password" or "--connection-string" or "--body" or "--version");
        Assert.Equal(0, await app.InvokeAsync(args));
    }

    [Fact]
    public async Task Install_RefusesReadingTwoSecretsFromStdinBeforeCreatingProject()
    {
        var directory = TestPaths.CreateScratchDirectory(nameof(Install_RefusesReadingTwoSecretsFromStdinBeforeCreatingProject));
        using var client = new HttpClient(new RejectNetworkHandler());
        var args = new[] { "install", directory, "--site-name", "Test", "--email", "admin@example.com", "--password-stdin", "--connection-string-stdin" };
        var app = await CliApplication.CreateAsync(args, new CliPaths(Path.Combine(directory, "config")), client, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());
        using var errors = new StringWriter();
        Assert.Equal(1, await app.InvokeAsync(args, errors));
        Assert.Contains("Only one secret can be read from stdin", errors.ToString());
        Assert.Empty(Directory.GetFiles(directory, "*.csproj"));
    }

    private static LocalSiteInstallOptions CreateOptions(string? directory = null) => new()
    {
        Directory = directory ?? TestPaths.CreateScratchDirectory("install-options"),
        SiteName = "Local site",
        UserName = "admin",
        Email = "admin@example.com",
    };

    private sealed class RejectNetworkHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Local install discovery must not contact a tenant.");
    }
}
