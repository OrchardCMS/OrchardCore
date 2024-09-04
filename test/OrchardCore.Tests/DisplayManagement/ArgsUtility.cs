using OrchardCore.DisplayManagement;
using Arguments = OrchardCore.DisplayManagement.Arguments;

namespace OrchardCore.Tests.DisplayManagement;

public static class ArgsUtility
{
    public static INamedEnumerable<T> Named<T>(IDictionary<string, T> args)
    {
        return FromDictionary(args);
    }

    public static INamedEnumerable<object> Named(object args)
    {
        return FromDictionary(new RouteValueDictionary(args));
    }
    public static INamedEnumerable<T> Empty<T>()
    {
        return Arguments.FromT(Array.Empty<T>(), []);
    }
    public static INamedEnumerable<object> Empty()
    {
        return Empty<object>();
    }

    private static INamedEnumerable<T> FromDictionary<T>(IDictionary<string, T> args)
    {
        return Arguments.FromT(args.Values, args.Keys);
    }

    public static INamedEnumerable<object> Positional(params object[] args)
    {
        return Arguments.From(args, []);
    }
}
