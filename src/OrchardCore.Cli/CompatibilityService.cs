using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli;

internal static class CompatibilityService
{
    public static CompatibilityOutput Evaluate(RemoteManagementManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        var cliVersion = CliUtilities.CliVersion;
        var output = new CompatibilityOutput
        {
            CliVersion = cliVersion,
            ExpectedProtocolMajor = RemoteManagementConstants.ProtocolMajorVersion,
            ExpectedProtocolMinor = RemoteManagementConstants.ProtocolMinorVersion,
            ManifestProtocolMajor = manifest.ProtocolMajorVersion,
            ManifestProtocolMinor = manifest.ProtocolMinorVersion,
            ProtocolCompatible = manifest.ProtocolMajorVersion == RemoteManagementConstants.ProtocolMajorVersion,
            ServerUsesNewerMinor = manifest.ProtocolMajorVersion == RemoteManagementConstants.ProtocolMajorVersion
                && manifest.ProtocolMinorVersion > RemoteManagementConstants.ProtocolMinorVersion,
            MinimumCliVersion = manifest.MinimumCliVersion,
            MinimumVersionSatisfied = CompareVersions(cliVersion, manifest.MinimumCliVersion) >= 0,
            RecommendedCliVersion = manifest.RecommendedCliVersion,
            RecommendedVersionSatisfied = CompareVersions(cliVersion, manifest.RecommendedCliVersion) >= 0,
        };

        foreach (var capability in manifest.Capabilities)
        {
            output.Capabilities.Add(new CapabilityCompatibilityOutput
            {
                Id = capability.Id,
                Version = capability.Version,
                MajorCompatible = IsMajorCompatible(capability.Version, RemoteManagementConstants.ProtocolMajorVersion),
            });
        }

        return output;
    }

    internal static int CompareVersions(string currentVersion, string? minimumVersion)
    {
        if (string.IsNullOrWhiteSpace(minimumVersion))
        {
            return 1;
        }

        if (!Version.TryParse(NormalizeVersion(currentVersion), out var current))
        {
            throw new CliException($"The CLI version '{currentVersion}' is invalid.");
        }

        if (!Version.TryParse(NormalizeVersion(minimumVersion), out var minimum))
        {
            throw new CliException($"The manifest version '{minimumVersion}' is invalid.");
        }

        return current.CompareTo(minimum);
    }

    private static bool IsMajorCompatible(string? version, int expectedMajor)
    {
        if (string.IsNullOrWhiteSpace(version) || !Version.TryParse(NormalizeVersion(version), out var parsed))
        {
            return false;
        }

        return parsed.Major == expectedMajor;
    }

    private static string NormalizeVersion(string version)
    {
        var trimmed = version.Trim();
        return trimmed.Count(static character => character == '.') switch
        {
            0 => $"{trimmed}.0.0",
            1 => $"{trimmed}.0",
            _ => trimmed,
        };
    }
}
