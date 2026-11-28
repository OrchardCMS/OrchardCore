using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization;

public readonly struct DeferredLocalizedString
{
    private readonly Type _resourceType;
    private readonly string _key;
    private readonly object[] _args;

    public DeferredLocalizedString(Type resourceType, string key, object[] args)
    {
        _resourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
        _key = key ?? throw new ArgumentNullException(nameof(key));
        _args = args ?? Array.Empty<object>();
    }

    public LocalizedString Value
    {
        get
        {
            var localizer = SharedStringLocalizer.GetLocalizer(_resourceType);

            return localizer[_key, _args];
        }
    }

    public override string ToString() => Value.Value;

    public static implicit operator string(DeferredLocalizedString lazy) => lazy.Value.Value;

    public static implicit operator LocalizedString(DeferredLocalizedString lazy) => lazy.Value;
}
