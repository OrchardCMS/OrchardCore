using System.Text.RegularExpressions;
using Fluid;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Tenants.Models;

namespace OrchardCore.Tenants.Services;

public partial class TenantDatabasePatternResolver
{
    private readonly TenantsOptions _tenantsOptions;
    private readonly FluidParser _fluidParser;

    protected readonly IStringLocalizer S;

    public TenantDatabasePatternResolver(
        FluidParser fluidParser,
        IOptions<TenantsOptions> tenantsOptions,
        IStringLocalizer<TenantDatabasePatternResolver> stringLocalizer)
    {
        _tenantsOptions = tenantsOptions.Value;
        _fluidParser = fluidParser;
        S = stringLocalizer;
    }

    public TenantDatabasePatternResolution Resolve(ShellSettings shellSettings)
    {
        var result = new TenantDatabasePatternResolution
        {
            HasTablePrefixPattern = !string.IsNullOrWhiteSpace(_tenantsOptions.TablePrefixPattern),
            HasSchemaPattern = !string.IsNullOrWhiteSpace(_tenantsOptions.SchemaPattern),
        };

        if (result.HasTablePrefixPattern)
        {
            result.TablePrefix = ResolvePattern(_tenantsOptions.TablePrefixPattern, shellSettings, "table prefix pattern", out var tablePrefixError);
            result.TablePrefixError = tablePrefixError;
        }

        if (result.HasSchemaPattern)
        {
            result.Schema = ResolvePattern(_tenantsOptions.SchemaPattern, shellSettings, "schema pattern", out var schemaError);
            result.SchemaError = schemaError;
        }

        return result;
    }

    public TenantDatabasePatternResolution Apply(TenantModelBase model)
    {
        var shellSettings = new ShellSettings
        {
            Name = model.Name,
            RequestUrlHost = model.RequestUrlHost,
            RequestUrlPrefix = model.RequestUrlPrefix,
        };

        var result = Resolve(shellSettings);

        if (result.HasTablePrefixPattern && string.IsNullOrEmpty(result.TablePrefixError))
        {
            model.TablePrefix = result.TablePrefix;
        }

        if (result.HasSchemaPattern && string.IsNullOrEmpty(result.SchemaError))
        {
            model.Schema = result.Schema;
        }

        return result;
    }

    internal static bool IsSqlIdentifier(string value) =>
        !string.IsNullOrWhiteSpace(value) &&
        SqlIdentifierRegex().IsMatch(value);

    private string ResolvePattern(string pattern, ShellSettings shellSettings, string optionDisplayName, out string error)
    {
        error = null;

        if (!_fluidParser.TryParse(pattern, out var template, out _))
        {
            error = S["The configured {0} is invalid.", optionDisplayName];
            return null;
        }

        var templateOptions = new TemplateOptions();
        templateOptions.MemberAccessStrategy.Register<ShellSettings>();
        var templateContext = new TemplateContext(templateOptions);
        templateContext.SetValue("ShellSettings", shellSettings);

        string value;

        try
        {
            value = template.Render(templateContext, NullEncoder.Default)
                .ReplaceLineEndings(string.Empty)
                .Trim();
        }
        catch
        {
            error = S["The configured {0} is invalid.", optionDisplayName];
            return null;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            error = S["The configured {0} resolved to an empty value.", optionDisplayName];
            return null;
        }

        if (!IsSqlIdentifier(value))
        {
            error = S["The configured {0} must resolve to a valid SQL identifier using only letters, numbers, and underscores, and it must start with a letter or underscore.", optionDisplayName];
            return null;
        }

        return value;
    }

    [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex SqlIdentifierRegex();
}
