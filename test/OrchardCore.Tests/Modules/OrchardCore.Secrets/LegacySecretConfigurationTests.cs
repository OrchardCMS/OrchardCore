using Fluid;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.AzureAI.Models;
using OrchardCore.AzureAI.Services;
using OrchardCore.Email;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Facebook.Settings;
using OrchardCore.GitHub.Settings;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Secrets;
using OrchardCore.Settings;
using OrchardCore.Sms.Azure.Models;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class LegacySecretConfigurationTests
{
    private const string Credential = "configured-sensitive-value";

    public static IEnumerable<object[]> ConfigurationCases()
    {
        (string Integration, string Section, string Key, string[] Aliases)[] integrations =
        [
            ("GitHub", "Authentication:GitHub", "ClientSecret", ["OrchardCore_GitHub"]),
            ("Facebook", "Facebook", "AppSecret", ["OrchardCore_Facebook"]),
            ("Google", "Authentication:Google", "ClientSecret", ["OrchardCore_Google"]),
            ("Microsoft", "Authentication:MicrosoftAccount", "AppSecret", ["OrchardCore_Microsoft_Authentication_MicrosoftAccount"]),
            ("Twitter", "X", "ConsumerSecret", ["OrchardCore_X", "OrchardCore_Twitter"]),
            ("Twitter", "X", "AccessTokenSecret", ["OrchardCore_X", "OrchardCore_Twitter"]),
            ("SMTP", "Email:Smtp", "Password", ["OrchardCore_Email_Smtp", "OrchardCore_Email"]),
            ("AzureEmail", "Email:Azure", "ConnectionString", ["OrchardCore_Email_Azure", "OrchardCore_Email_AzureCommunicationServices"]),
            ("AzureSms", "Sms:Azure", "ConnectionString", ["OrchardCore_Sms_AzureCommunicationServices"]),
            ("AzureAI", "Search:AzureAISearch", "Credential:Key", ["OrchardCore_AzureAISearch"]),
        ];

        foreach (var (integration, section, key, aliases) in integrations)
        {
            foreach (var source in new[] { section }.Concat(aliases).Concat(aliases.Select(alias => alias.Replace('_', '.'))))
            {
                yield return [integration, section, key, source];
            }
        }
    }

    [Theory]
    [MemberData(nameof(ConfigurationCases))]
    public void LoadingConfiguration_WarnsWithoutExposingValue(string integration, string section, string key, string source)
    {
        var logger = CreateLogger();
        var factory = new Mock<ILoggerFactory>();
        factory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
        var configuration = new ShellConfiguration(new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string>
            {
                [source + ":" + key] = Credential,
                [section + ":DisableUIConfiguration"] = "true",
                [section + ":Endpoint"] = "https://example.search.windows.net",
            }).Build());

        var applicationServices = new ServiceCollection();
        applicationServices.AddSingleton<IShellConfiguration>(configuration);
        var builder = new OrchardCoreBuilder(applicationServices);
        ConfigureAuthentication(builder, integration);
        using var application = applicationServices.BuildServiceProvider();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(factory.Object);
        services.AddSingleton(new ShellSettings { Name = "TenantA" });
        services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
        services.AddSingleton(new FluidParser());
        services.AddSingleton(Options.Create(new ShellOptions()));
        services.AddSingleton<IShellConfiguration>(configuration);
        services.AddSingleton(Mock.Of<ISecretManager>());
        services.AddSingleton(Mock.Of<ISiteService>());
        foreach (var startup in application.GetServices<global::OrchardCore.Modules.IStartup>())
        {
            startup.ConfigureServices(services);
        }

        switch (integration)
        {
            case "SMTP":
                new global::OrchardCore.Email.Smtp.Startup(configuration).ConfigureServices(services);
                break;
            case "AzureEmail":
                new global::OrchardCore.Email.Azure.Startup(configuration).ConfigureServices(services);
                break;
            case "AzureSms":
                new global::OrchardCore.Sms.Azure.Startup(configuration).ConfigureServices(services);
                break;
            case "AzureAI":
                services.AddTransient<IConfigureOptions<AzureAISearchDefaultOptions>, AzureAISearchDefaultOptionsConfigurations>();
                break;
        }

        using var provider = services.BuildServiceProvider();
        var actual = ResolveCredential(provider, integration, key);

        var protectionPurpose = integration switch
        {
            "Facebook" => global::OrchardCore.Facebook.FacebookConstants.Features.Core,
            "Google" => global::OrchardCore.Google.GoogleConstants.Features.GoogleAuthentication,
            "Microsoft" => global::OrchardCore.Microsoft.Authentication.MicrosoftAuthenticationConstants.Features.MicrosoftAccount,
            "Twitter" => global::OrchardCore.Twitter.TwitterConstants.Features.Twitter,
            _ => null,
        };
        Assert.Equal(Credential, protectionPurpose == null
            ? actual
            : provider.GetRequiredService<IDataProtectionProvider>().CreateProtector(protectionPurpose).Unprotect(actual));

        factory.Verify(f => f.CreateLogger(SecretConfigurationExtensions.LoggerCategory), Times.Once);
        var invocation = Assert.Single(logger.Invocations, call => call.Method.Name == nameof(ILogger.Log));
        Assert.Equal(LogLevel.Warning, invocation.Arguments[0]);
        Assert.Equal(8100, ((EventId)invocation.Arguments[1]).Id);
        Assert.Equal("LegacySecretConfiguration", ((EventId)invocation.Arguments[1]).Name);
        var message = invocation.Arguments[2].ToString();
        Assert.Contains("LegacySecretConfiguration", message);
        Assert.Contains("TenantA", message);
        Assert.Contains(section + ":" + key, message);
        Assert.Contains("future version", message);
        Assert.DoesNotContain(Credential, message);
        Assert.DoesNotContain(Credential, string.Join(" ", ((IEnumerable<KeyValuePair<string, object>>)invocation.Arguments[2]).Select(item => item.Value)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void EmptyCredential_DoesNotWarn(string value)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string> { ["Email:Smtp:Password"] = value }).Build();
        var logger = CreateLogger();

        configuration.GetSection("Email:Smtp").WarnIfLegacySecretConfigured(logger.Object, "TenantA", "SMTP", "Password");

        Assert.DoesNotContain(logger.Invocations, call => call.Method.Name == nameof(ILogger.Log));
    }

    [Fact]
    public void SecretReferenceWithoutLegacyValue_DoesNotWarn()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string> { ["Authentication:Google:ClientSecretSecretName"] = "Google.ClientSecret" }).Build();
        var logger = CreateLogger();

        configuration.GetSection("Authentication:Google").WarnIfLegacySecretConfigured(logger.Object, "TenantA", "Google", "ClientSecret");

        Assert.DoesNotContain(logger.Invocations, call => call.Method.Name == nameof(ILogger.Log));
    }

    [Fact]
    public void LegacyValueAlongsideReference_StillWarns()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["Authentication:Google:ClientSecret"] = Credential,
            ["Authentication:Google:ClientSecretSecretName"] = "Google.ClientSecret",
        }).Build();
        var logger = CreateLogger();

        configuration.GetSection("Authentication:Google").WarnIfLegacySecretConfigured(logger.Object, "TenantA", "Google", "ClientSecret");

        Assert.Single(logger.Invocations, call => call.Method.Name == nameof(ILogger.Log));
    }

    [Fact]
    public void CurrentEmptyValueOverridesLegacyValue_DoesNotWarn()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["Authentication:Google:ClientSecret"] = string.Empty,
            ["OrchardCore_Google:ClientSecret"] = Credential,
        }).Build();
        var logger = CreateLogger();

        configuration.GetSectionCompat("Authentication:Google", "OrchardCore_Google")
            .WarnIfLegacySecretConfigured(logger.Object, "TenantA", "Google", "ClientSecret");

        Assert.DoesNotContain(logger.Invocations, call => call.Method.Name == nameof(ILogger.Log));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(null, true)]
    [InlineData(Credential, false)]
    [InlineData(Credential, true)]
    public void GoogleConfiguration_PreservesSettingsAndOnlyWarnsForConfiguredValue(string configuredValue, bool useReference)
    {
        var configuration = new ShellConfiguration(new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string> { ["Authentication:Google:ClientSecret"] = configuredValue }).Build());
        var logger = CreateLogger();
        var factory = new Mock<ILoggerFactory>();
        factory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
        var protection = new EphemeralDataProtectionProvider();
        var protector = protection.CreateProtector(global::OrchardCore.Google.GoogleConstants.Features.GoogleAuthentication);
        var protectedValue = protector.Protect(Credential);
        var applicationServices = new ServiceCollection();
        applicationServices.AddSingleton<IShellConfiguration>(configuration);
        new OrchardCoreBuilder(applicationServices).ConfigureGoogleSettings();
        using var application = applicationServices.BuildServiceProvider();
        var services = new ServiceCollection();
        services.AddSingleton(factory.Object);
        services.AddSingleton(new ShellSettings { Name = "TenantA" });
        services.AddSingleton<IDataProtectionProvider>(protection);
        services.Configure<GoogleAuthenticationSettings>(settings =>
        {
#pragma warning disable CS0618 // Model legacy tenant settings separately from configuration.
            settings.ClientSecret = protectedValue;
#pragma warning restore CS0618
            settings.ClientSecretSecretName = useReference ? "Selected.Secret" : null;
        });
        foreach (var startup in application.GetServices<global::OrchardCore.Modules.IStartup>())
        {
            startup.ConfigureServices(services);
        }

        using var provider = services.BuildServiceProvider();
        var settings = provider.GetRequiredService<IOptions<GoogleAuthenticationSettings>>().Value;

#pragma warning disable CS0618 // Verify that the legacy credential remains protected.
        Assert.Equal(Credential, protector.Unprotect(settings.ClientSecret));
#pragma warning restore CS0618
        Assert.Equal(useReference ? "Selected.Secret" : null, settings.ClientSecretSecretName);
        Assert.Equal(configuredValue == null ? 0 : 1, logger.Invocations.Count(call => call.Method.Name == nameof(ILogger.Log)));
    }

    [Fact]
    public void DisabledWarnings_DoNotLog()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string> { ["Email:Smtp:Password"] = Credential }).Build();
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(false);

        configuration.GetSection("Email:Smtp").WarnIfLegacySecretConfigured(logger.Object, "TenantA", "SMTP", "Password");

        Assert.DoesNotContain(logger.Invocations, call => call.Method.Name == nameof(ILogger.Log));
    }

    [Fact]
    public void MultipleCredentials_WarnForEachKeyWithoutValues()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["X:ConsumerSecret"] = Credential,
            ["X:AccessTokenSecret"] = Credential,
        }).Build();
        var logger = CreateLogger();

        configuration.GetSection("X").WarnIfLegacySecretConfigured(logger.Object, "TenantA", "Twitter", "ConsumerSecret", "AccessTokenSecret");

        var messages = logger.Invocations.Where(call => call.Method.Name == nameof(ILogger.Log))
            .Select(call => call.Arguments[2].ToString()).ToArray();
        Assert.Equal(2, messages.Length);
        Assert.Contains(messages, message => message.Contains("X:ConsumerSecret", StringComparison.Ordinal));
        Assert.Contains(messages, message => message.Contains("X:AccessTokenSecret", StringComparison.Ordinal));
        Assert.All(messages, message => Assert.DoesNotContain(Credential, message));
    }

    private static Mock<ILogger> CreateLogger()
    {
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);
        return logger;
    }

    private static void ConfigureAuthentication(OrchardCoreBuilder builder, string integration)
    {
        switch (integration)
        {
            case "GitHub":
                builder.ConfigureGitHubSettings();
                break;
            case "Facebook":
                builder.ConfigureFacebookSettings();
                break;
            case "Google":
                builder.ConfigureGoogleSettings();
                break;
            case "Microsoft":
                builder.ConfigureMicrosoftAccountSettings();
                break;
            case "Twitter":
                builder.ConfigureTwitterSettings();
                break;
        }
    }

#pragma warning disable CS0618 // Verify that legacy configuration binding and protection are preserved.
    private static string ResolveCredential(IServiceProvider provider, string integration, string key) => integration switch
    {
        "GitHub" => provider.GetRequiredService<IOptions<GitHubAuthenticationSettings>>().Value.ClientSecret,
        "Facebook" => provider.GetRequiredService<IOptions<FacebookSettings>>().Value.AppSecret,
        "Google" => provider.GetRequiredService<IOptions<GoogleAuthenticationSettings>>().Value.ClientSecret,
        "Microsoft" => provider.GetRequiredService<IOptions<MicrosoftAccountSettings>>().Value.AppSecret,
        "Twitter" when key == "ConsumerSecret" => provider.GetRequiredService<IOptions<TwitterSettings>>().Value.ConsumerSecret,
        "Twitter" => provider.GetRequiredService<IOptions<TwitterSettings>>().Value.AccessTokenSecret,
        "SMTP" => provider.GetRequiredService<IOptions<DefaultSmtpOptions>>().Value.Password,
        "AzureEmail" => provider.GetRequiredService<IOptions<DefaultAzureEmailOptions>>().Value.ConnectionString,
        "AzureSms" => provider.GetRequiredService<IOptions<DefaultAzureSmsOptions>>().Value.ConnectionString,
        "AzureAI" => provider.GetRequiredService<IOptions<AzureAISearchDefaultOptions>>().Value.Credential.Key,
        _ => throw new ArgumentOutOfRangeException(nameof(integration)),
    };
#pragma warning restore CS0618
}
