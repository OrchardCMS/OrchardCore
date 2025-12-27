namespace OrchardCore;

public static class TypeExtensions
{
    public static bool IsSubclassOfRawGeneric(this Type type, Type generic)
    {
        if (type == null || generic == null)
        {
            throw new ArgumentNullException(type == null ? nameof(type) : nameof(generic));
        }

        if (!generic.IsGenericTypeDefinition)
        {
            throw new ArgumentException("The 'generic' parameter must be an open generic type definition.", nameof(generic));
        }

        while (type != null && type != typeof(object))
        {
            var current = type.IsGenericType
                ? type.GetGenericTypeDefinition()
                : type;
            if (current == generic)
            {
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }
}
