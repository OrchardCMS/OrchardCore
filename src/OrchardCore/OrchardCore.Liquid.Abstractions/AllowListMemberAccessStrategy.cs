using Fluid;

namespace OrchardCore.Liquid;

/// <summary>
/// A <see cref="MemberAccessStrategy"/> that only exposes the members of the types that were explicitly registered,
/// such that a template can't read everything that is reachable from its model, e.g. a service or a secret, as it
/// can with <see cref="DefaultMemberAccessStrategy"/> which falls back to reflection for any other member.
/// </summary>
/// <remarks>
/// A member is looked up on the type of the object, then on its base types and on its interfaces, and its name is
/// case-sensitive. The registrations are rejected once a member has been looked up.
/// </remarks>
public sealed class AllowListMemberAccessStrategy : MemberAccessStrategy
{
    private readonly Dictionary<(Type Type, string Name), MemberAccessor> _accessors = [];
    private volatile bool _isReadOnly;

    // The accessors don't change once they are read, so the template call sites can remember them.
    protected override object AccessorCacheToken => _accessors;

    public override MemberAccessor GetAccessor(Type type, string name, StringComparer stringComparer)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(name);

        if (!_isReadOnly)
        {
            _isReadOnly = true;
        }

        if (TryGetAccessor(type, name, stringComparer, out var accessor))
        {
            return accessor;
        }

        for (var baseType = type.BaseType; baseType != null && baseType != typeof(object); baseType = baseType.BaseType)
        {
            if (TryGetAccessor(baseType, name, stringComparer, out accessor))
            {
                return accessor;
            }
        }

        foreach (var interfaceType in type.GetInterfaces())
        {
            if (TryGetAccessor(interfaceType, name, stringComparer, out accessor))
            {
                return accessor;
            }
        }

        return null;
    }

    public override void Register(Type type, string name, MemberAccessor accessor)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(name);

        if (_isReadOnly)
        {
            throw new InvalidOperationException($"The registrations can't be changed anymore, use '{nameof(TemplateOptionsBuilder)}.{nameof(TemplateOptionsBuilder.ConfigureMemberAccess)}()'.");
        }

        _accessors[(type, name)] = accessor;
    }

    private bool TryGetAccessor(Type type, string name, StringComparer stringComparer, out MemberAccessor accessor)
    {
        if (_accessors.TryGetValue((type, name), out accessor) || _accessors.TryGetValue((type, "*"), out accessor))
        {
            accessor = accessor.ResolveMember(name, stringComparer);

            return true;
        }

        return false;
    }
}
