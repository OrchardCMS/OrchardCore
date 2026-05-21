using Fluid;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Email.Smtp.Services;

internal static class SmtpPickupDirectoryResolver
{
    public const string DefaultPickupDirectoryLocation = "/";
    public const string DefaultPickupDirectoryLocationBaseTemplate = @"{{ AppData }}\Sites\{{ ShellSettings.Name }}\Emails";

    private static readonly char[] _invalidPickupDirectoryLocationCharacters =
    [
        ..Path.GetInvalidPathChars(),
        '~',
        '{',
        '}',
    ];

    public static void ConfigurePickupDirectory(
        SmtpOptions options,
        string configuredBasePath,
        FluidParser fluidParser,
        ShellOptions shellOptions,
        ShellSettings shellSettings)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.PickupDirectoryLocationBase = GetPickupDirectoryLocationBase(
            configuredBasePath,
            fluidParser,
            shellOptions,
            shellSettings);

        options.PickupDirectoryLocation = ResolvePickupDirectoryLocation(
            options.PickupDirectoryLocationBase,
            options.PickupDirectoryLocation);
    }

    public static string GetPickupDirectoryLocationBase(string configuredBasePath, FluidParser fluidParser, ShellOptions shellOptions, ShellSettings shellSettings)
    {
        ArgumentNullException.ThrowIfNull(fluidParser);
        ArgumentNullException.ThrowIfNull(shellOptions);
        ArgumentNullException.ThrowIfNull(shellSettings);

        var pickupDirectoryLocationBase = string.IsNullOrWhiteSpace(configuredBasePath)
            ? DefaultPickupDirectoryLocationBaseTemplate
            : configuredBasePath;

        var formattedPickupDirectoryLocationBase = ParseAndFormat(pickupDirectoryLocationBase, fluidParser, shellOptions, shellSettings);

        return Path.GetFullPath(formattedPickupDirectoryLocationBase);
    }

    public static string ResolvePickupDirectoryLocation(string pickupDirectoryLocationBase, string pickupDirectoryLocation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pickupDirectoryLocationBase);

        if (!IsValidPickupDirectoryLocation(pickupDirectoryLocation))
        {
            throw new InvalidOperationException("The SMTP pickup directory location is invalid.");
        }

        var normalizedPickupDirectoryLocation = NormalizePickupDirectoryLocation(pickupDirectoryLocation);

        var fullPickupDirectoryLocation = string.IsNullOrWhiteSpace(normalizedPickupDirectoryLocation)
            ? pickupDirectoryLocationBase
            : Path.GetFullPath(Path.Combine(pickupDirectoryLocationBase, normalizedPickupDirectoryLocation));

        if (!IsWithinBasePath(pickupDirectoryLocationBase, fullPickupDirectoryLocation))
        {
            throw new InvalidOperationException($"The SMTP pickup directory location '{pickupDirectoryLocation}' resolves outside the configured pickup directory base '{pickupDirectoryLocationBase}'.");
        }

        return fullPickupDirectoryLocation;
    }

    public static bool IsValidPickupDirectoryLocation(string pickupDirectoryLocation)
    {
        if (string.IsNullOrWhiteSpace(pickupDirectoryLocation))
        {
            return true;
        }

        if (pickupDirectoryLocation.IndexOfAny(_invalidPickupDirectoryLocationCharacters) >= 0 ||
            pickupDirectoryLocation.Contains("{%", StringComparison.Ordinal))
        {
            return false;
        }

        if (ContainsNavigationSegments(pickupDirectoryLocation))
        {
            return false;
        }

        if (Path.IsPathFullyQualified(pickupDirectoryLocation) ||
            pickupDirectoryLocation.StartsWith(@"\\", StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }

    private static string NormalizePickupDirectoryLocation(string pickupDirectoryLocation)
    {
        if (string.IsNullOrWhiteSpace(pickupDirectoryLocation))
        {
            return null;
        }

        pickupDirectoryLocation = pickupDirectoryLocation
            .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            .Trim();

        if (pickupDirectoryLocation == DefaultPickupDirectoryLocation ||
            pickupDirectoryLocation == Path.DirectorySeparatorChar.ToString())
        {
            return null;
        }

        return pickupDirectoryLocation.TrimStart(Path.DirectorySeparatorChar);
    }

    private static bool ContainsNavigationSegments(string pickupDirectoryLocation)
    {
        var normalizedPath = pickupDirectoryLocation.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        return normalizedPath
            .Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(segment => segment is "." or "..");
    }

    private static string ParseAndFormat(string template, FluidParser fluidParser, ShellOptions shellOptions, ShellSettings shellSettings)
    {
        var templateOptions = new TemplateOptions();
        templateOptions.MemberAccessStrategy.Register<ShellSettings>();

        if (!fluidParser.TryParse(template, out var parsedTemplate, out var errors))
        {
            throw new InvalidOperationException($"Failed to parse SMTP pickup directory location base: {string.Join(System.Environment.NewLine, errors)}");
        }

        var templateContext = new TemplateContext(templateOptions);
        templateContext.SetValue("AppData", shellOptions.ShellsApplicationDataPath);
        templateContext.SetValue("ShellSettings", shellSettings);

        return parsedTemplate.Render(templateContext, NullEncoder.Default)
            .ReplaceLineEndings(string.Empty)
            .Trim();
    }

    private static bool IsWithinBasePath(string pickupDirectoryLocationBase, string pickupDirectoryLocation)
    {
        var relativePath = Path.GetRelativePath(pickupDirectoryLocationBase, pickupDirectoryLocation);

        return relativePath == "." ||
            (!relativePath.Equals("..", StringComparison.Ordinal) &&
             !relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
             !Path.IsPathRooted(relativePath));
    }
}
