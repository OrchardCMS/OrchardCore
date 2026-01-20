using Microsoft.AspNetCore.Mvc.Razor.Extensions;

namespace Microsoft.AspNetCore.Razor.Hosting;

public class TenantRazorCompiledItem : RazorCompiledItem
{
    private object[] _metadata;

    public TenantRazorCompiledItem(Type type, string kind, string identifier)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(kind);
        ArgumentNullException.ThrowIfNull(identifier);

        Type = type;
        Kind = kind;
        Identifier = identifier;
    }

    public TenantRazorCompiledItem(Type type, string identifier)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(identifier);

        Type = type;
        Kind = MvcViewDocumentClassifierPass.MvcViewDocumentKind;
        Identifier = identifier;
    }

    public override string Identifier { get; }

    public override string Kind { get; }

    public override IReadOnlyList<object> Metadata => _metadata ??= Type.GetCustomAttributes(inherit: true);

    public override Type Type { get; }
}
