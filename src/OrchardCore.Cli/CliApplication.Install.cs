using System.CommandLine;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli;

internal sealed partial class CliApplication
{
    private Command CreateInstallCommand()
    {
        var command = new Command("install", "Create a local CMS from the embedded template and initialize its Default tenant");
        var directory = new Argument<DirectoryInfo>("directory") { Description = "New or empty project directory" };
        var siteName = new Option<string>("--site-name") { Description = "Site display name", Required = true };
        var userName = new Option<string>("--user-name") { Description = "Administrator user name", DefaultValueFactory = _ => "admin" };
        var email = new Option<string>("--email") { Description = "Administrator email address", Required = true };
        var recipe = new Option<string>("--recipe-name") { Description = "Setup recipe, for example Blog for a blog, SaaS for a SaaS site, or Blank for a minimal site", DefaultValueFactory = _ => "SaaS" };
        var provider = new Option<string>("--database-provider") { Description = "Database provider", DefaultValueFactory = _ => "Sqlite" };
        var tablePrefix = new Option<string?>("--table-prefix") { Description = "Database table prefix" };
        var schema = new Option<string?>("--schema") { Description = "Database schema" };
        var timeZone = new Option<string>("--site-time-zone") { Description = "IANA/TZDB time zone ID, for example America/Los_Angeles, Europe/Paris, or UTC", DefaultValueFactory = _ => "UTC" };
        var urlPrefix = new Option<string?>("--request-url-prefix") { Description = "Optional site URL path prefix" };
        var host = new Option<string?>("--request-url-host") { Description = "Optional single site host name" };
        var source = new Option<string?>("--source") { Description = "Additional NuGet source for Orchard dependencies; inherited sources remain available" };
        var clearSources = new Option<bool>("--clear-sources") { Description = "Clear inherited NuGet package sources; use only nuget.org and an explicit --source" };
        var enableRemoteManagement = new Option<bool>("--enable-remote-management") { Description = "Enable Remote Management CLI and OpenID, and save a context with credentials for a new administrative application" };
        var run = new Option<bool>("--run") { Description = "Start the completed site in the foreground; Ctrl+C stops it" };
        var verbose = new Option<bool>("--verbose") { Description = "Show template, build, and temporary setup logs as they happen" };
        var urls = new Option<string?>("--urls") { Description = "Listen URLs (default: HTTPS on an available random localhost port); quote multiple addresses separated by semicolons, e.g. \"https://localhost:5001;http://localhost:5000\". HTTPS requires a certificate" };
        var timeout = new Option<int>("--setup-timeout") { Description = "Setup timeout in seconds", DefaultValueFactory = _ => 300 };
        command.Arguments.Add(directory);
        foreach (var option in new Option[] { siteName, userName, email, recipe, provider, tablePrefix, schema, timeZone, urlPrefix, host, source, clearSources, enableRemoteManagement, run, verbose, urls, timeout })
        {
            command.Options.Add(option);
        }

        var password = AddInstallSecretOptions(command, "password", "administrator password");
        var connection = AddInstallSecretOptions(command, "connectionString", "database connection string");
        command.SetAction(async (parsed, cancellationToken) =>
        {
            if (parsed.GetValue(password.StdinOption) && parsed.GetValue(connection.StdinOption))
            {
                throw new CliException("Only one secret can be read from stdin. Use an environment variable or file for the other secret.");
            }

            var options = new LocalSiteInstallOptions
            {
                Directory = parsed.GetValue(directory)!.FullName,
                SiteName = parsed.GetValue(siteName)!,
                UserName = parsed.GetValue(userName)!,
                Email = parsed.GetValue(email)!,
                RecipeName = parsed.GetValue(recipe)!,
                DatabaseProvider = parsed.GetValue(provider)!,
                TablePrefix = parsed.GetValue(tablePrefix),
                Schema = parsed.GetValue(schema),
                SiteTimeZone = parsed.GetValue(timeZone)!,
                RequestUrlPrefix = parsed.GetValue(urlPrefix),
                RequestUrlHost = parsed.GetValue(host),
                Source = parsed.GetValue(source),
                ClearSources = parsed.GetValue(clearSources),
                Urls = parsed.GetValue(urls),
                SetupTimeoutSeconds = parsed.GetValue(timeout),
                Verbose = parsed.GetValue(verbose),
                ClientCredentials = parsed.GetValue(enableRemoteManagement) ? RemoteManagementClientCredentials.Generate() : null,
                SecretEnvironmentVariables = new[] { parsed.GetValue(password.EnvironmentVariableOption), parsed.GetValue(connection.EnvironmentVariableOption) }
                    .Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!).ToArray(),
            };
            LocalSiteInstaller.Validate(options);
            if (options.Urls is not null)
            {
                // Report occupied explicit ports before SDK discovery or password prompts.
                using var preflight = InstallPortReservation.Create(options.Urls);
            }
            var sdk = await DotnetEnvironment.FindSdkAsync(cancellationToken, options.SecretEnvironmentVariables)
                ?? throw new CliException($"Install the .NET {DotnetEnvironment.RequiredMajor} SDK and make 'dotnet' available on PATH to use 'pomi install'. Remote commands do not require it.");
            options.Password = await ResolveSecretValueAsync(parsed, password, cancellationToken) ?? ReadSecretFromConsole("password");
            options.ConnectionString = await ResolveSecretValueAsync(parsed, connection, cancellationToken);
            LocalSiteInstaller.ValidateSecrets(options);
            var output = await LocalSiteInstaller.InstallAsync(options, sdk, Console.Error, cancellationToken);
            if (options.ClientCredentials is { } credentials)
            {
                output.Context = await SaveProvisionedContextAsync(Path.GetFileName(Path.TrimEndingDirectorySeparator(output.Directory)), output.Url,
                    credentials.ClientId, credentials.ClientSecret, cancellationToken);
            }

            var exitCode = await WriteOutputAsync(parsed, CliUtilities.ToJsonElement(output, CliJsonContext.Default.LocalSiteInstallOutput), cancellationToken);
            if (parsed.GetValue(run))
            {
                await LocalSiteInstaller.RunAsync(output, Console.Error, cancellationToken, options.SecretEnvironmentVariables);
            }

            return exitCode;
        });
        return command;
    }

    private static SecretBodyPropertyOptions AddInstallSecretOptions(Command command, string name, string description)
    {
        var cliName = CliUtilities.ToCliName(name);
        var options = new SecretBodyPropertyOptions
        {
            Property = new RequestBodyPropertyDefinition { Name = name },
            EnvironmentVariableOption = new Option<string?>($"--{cliName}-env") { Description = $"Environment variable containing the {description}" },
            FileOption = new Option<FileInfo?>($"--{cliName}-file") { Description = $"Read the {description} from a file" },
            StdinOption = new Option<bool>($"--{cliName}-stdin") { Description = $"Read the {description} from stdin" },
        };
        command.Options.Add(options.EnvironmentVariableOption);
        command.Options.Add(options.FileOption);
        command.Options.Add(options.StdinOption);
        return options;
    }
}
