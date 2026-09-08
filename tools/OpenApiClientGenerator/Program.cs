using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;

const string RecipeName = "OpenApiGenerationSetup";
const string NSwagConfigRelativePath = "src/OrchardCore.Modules/OrchardCore.OpenApi/OrchardCore.OpenApi.nswag";

var repoRoot = FindRepoRoot(AppContext.BaseDirectory);
var contentRoot = Path.Combine(repoRoot, "src", "OrchardCore.Cms.Web");
var nswagConfigPath = Path.Combine(repoRoot, NSwagConfigRelativePath);

if (!File.Exists(nswagConfigPath))
{
    Console.Error.WriteLine($"Could not find NSwag config at '{nswagConfigPath}'.");
    return 1;
}

var appDataPath = Path.Combine(Path.GetTempPath(), $"openapi-client-generator-{Guid.NewGuid():N}");
Directory.CreateDirectory(appDataPath);

var scratchSwaggerJsonPath = Path.Combine(appDataPath, "swagger.json");
var scratchNSwagConfigPath = Path.Combine(Path.GetDirectoryName(nswagConfigPath)!, $".scratch-{Guid.NewGuid():N}.nswag");

WebApplication? app = null;

try
{
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        ApplicationName = "OrchardCore.Cms.Web",
        ContentRootPath = contentRoot,
    });

    builder.WebHost.UseUrls("http://127.0.0.1:0");
    builder.WebHost.UseSetting("suppressHostingStartup", "true");

    builder.Services.PostConfigure<ShellOptions>(options =>
    {
        options.ShellsApplicationDataPath = appDataPath;
    });

    // Configure OrchardCore.AutoSetup to provision the Default tenant headlessly with the
    // OpenApiGenerationSetup recipe (src/OrchardCore.Cms.Web/Recipes/openapi-generation-setup.recipe.json),
    // which enables the same feature set as the OpenApiGeneration recipe so the generated
    // swagger.json covers the module's full API surface.
    var autoSetup = new Dictionary<string, string?>
    {
        ["OrchardCore:OrchardCore_AutoSetup:Tenants:0:ShellName"] = ShellSettings.DefaultShellName,
        ["OrchardCore:OrchardCore_AutoSetup:Tenants:0:SiteName"] = "OpenApi Client Generator",
        ["OrchardCore:OrchardCore_AutoSetup:Tenants:0:SiteTimeZone"] = "Etc/UTC",
        ["OrchardCore:OrchardCore_AutoSetup:Tenants:0:AdminUsername"] = "admin",
        ["OrchardCore:OrchardCore_AutoSetup:Tenants:0:AdminEmail"] = "admin@example.com",
        ["OrchardCore:OrchardCore_AutoSetup:Tenants:0:AdminPassword"] = $"P@ss{Guid.NewGuid():N}",
        ["OrchardCore:OrchardCore_AutoSetup:Tenants:0:DatabaseProvider"] = "Sqlite",
        ["OrchardCore:OrchardCore_AutoSetup:Tenants:0:RecipeName"] = RecipeName,
    };
    builder.Configuration.AddInMemoryCollection(autoSetup);

    builder.Services
        .AddOrchardCms()
        .AddSetupFeatures("OrchardCore.AutoSetup");

    app = builder.Build();
    app.UseStaticFiles();
    app.UseOrchardCore();

    await app.StartAsync();

    var address = GetListeningAddress(app);
    Console.WriteLine($"CMS started at {address}. Waiting for AutoSetup + swagger.json...");

    var json = await WaitForSwaggerJsonAsync($"{address}/swagger/v1/swagger.json", timeoutSeconds: 120);
    await File.WriteAllTextAsync(scratchSwaggerJsonPath, json);
    Console.WriteLine($"Captured swagger.json ({json.Length} bytes).");

    WriteScratchNSwagConfig(nswagConfigPath, scratchNSwagConfigPath, scratchSwaggerJsonPath);

    var exitCode = await RunNSwagAsync(scratchNSwagConfigPath, Path.GetDirectoryName(nswagConfigPath)!);
    if (exitCode != 0)
    {
        Console.Error.WriteLine($"nswag exited with code {exitCode}.");
        return exitCode;
    }

    Console.WriteLine("NSwag clients regenerated successfully.");
    return 0;
}
finally
{
    if (app is not null)
    {
        await app.StopAsync();
        await app.DisposeAsync();
    }

    TryDelete(scratchNSwagConfigPath);
    TryDeleteDirectory(appDataPath);
}

static string FindRepoRoot(string startDirectory)
{
    var dir = new DirectoryInfo(startDirectory);
    while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "OrchardCore.slnx")))
    {
        dir = dir.Parent;
    }

    return dir?.FullName
        ?? throw new InvalidOperationException($"Could not locate the repository root (OrchardCore.slnx) above '{startDirectory}'.");
}

static string GetListeningAddress(WebApplication app)
{
    var server = app.Services.GetRequiredService<IServer>();
    var addresses = server.Features.Get<IServerAddressesFeature>();
    return addresses!.Addresses.First();
}

static async Task<string> WaitForSwaggerJsonAsync(string url, int timeoutSeconds)
{
    using var client = new HttpClient();
    var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
    Exception? lastError = null;
    System.Net.HttpStatusCode? lastStatus = null;

    while (DateTime.UtcNow < deadline)
    {
        try
        {
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            lastStatus = response.StatusCode;
        }
        catch (Exception ex)
        {
            lastError = ex;
        }

        await Task.Delay(1000);
    }

    // A persistent 401 usually means the setup recipe's settings step did not enable
    // AllowAnonymousSchemaAccess (e.g. the step was reordered before the feature step
    // and silently skipped) — surface the status so that is diagnosable.
    var lastStatusHint = lastStatus is not null
        ? $" Last response status: {(int)lastStatus} {lastStatus}."
        : string.Empty;

    throw new TimeoutException(
        $"'{url}' did not return a successful response within {timeoutSeconds} seconds.{lastStatusHint}", lastError);
}

static void WriteScratchNSwagConfig(string realConfigPath, string scratchConfigPath, string swaggerJsonPath)
{
    var json = File.ReadAllText(realConfigPath);
    var node = JsonNode.Parse(json)!;
    node["documentGenerator"]!["fromDocument"]!["url"] = swaggerJsonPath;
    File.WriteAllText(scratchConfigPath, node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
}

static async Task<int> RunNSwagAsync(string scratchConfigPath, string workingDirectory)
{
    var nswagExecutable = ResolveNSwagExecutable();

    var startInfo = new ProcessStartInfo
    {
        FileName = nswagExecutable,
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };
    startInfo.ArgumentList.Add("run");
    startInfo.ArgumentList.Add(Path.GetFileName(scratchConfigPath));

    using var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException($"Failed to start '{nswagExecutable}'.");

    process.OutputDataReceived += (_, e) =>
    {
        if (e.Data is not null)
        {
            Console.WriteLine(e.Data);
        }
    };
    process.ErrorDataReceived += (_, e) =>
    {
        if (e.Data is not null)
        {
            Console.Error.WriteLine(e.Data);
        }
    };
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    await process.WaitForExitAsync();
    return process.ExitCode;
}

static string ResolveNSwagExecutable()
{
    // Prefer the dotnet-tools-installed CLI (documented prerequisite: `dotnet tool install -g NSwag.ConsoleCore`).
    var dotnetToolsPath = Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
        ".dotnet", "tools", OperatingSystem.IsWindows() ? "nswag.exe" : "nswag");

    return File.Exists(dotnetToolsPath) ? dotnetToolsPath : "nswag";
}

static void TryDelete(string path)
{
    try
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
    catch
    {
        // Best-effort cleanup.
    }
}

static void TryDeleteDirectory(string path)
{
    try
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }
    catch
    {
        // Best-effort cleanup.
    }
}
