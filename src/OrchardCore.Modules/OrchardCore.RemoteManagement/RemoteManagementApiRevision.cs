using System.Security.Cryptography;
using System.Text;
using OrchardCore.Environment.Shell.Builders.Models;

namespace OrchardCore.RemoteManagement;

// Captured when the tenant pipeline is built: a feature mutation must not label
// the old pipeline's OpenAPI document with the next shell's revision.
internal sealed class RemoteManagementApiRevision
{
    public RemoteManagementApiRevision(ShellBlueprint blueprint)
    {
        var features = blueprint.Descriptor.Features.Select(feature => feature.Id)
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal);
        var modules = blueprint.Dependencies.Keys.Select(type => type.Module.ModuleVersionId.ToString("D"))
            .Append(typeof(RemoteManagementApiRevision).Module.ModuleVersionId.ToString("D"))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal);
        Value = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(
            string.Join('\n', features) + "\n--modules--\n" + string.Join('\n', modules))));
    }

    public string Value { get; }
}
