using System.Collections.Generic;
using GraphQL.Builders;
using GraphQL.Types;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL
{
    public static class PermissionsExtensions
    {
        public static readonly string PermissionsKey = "Permissions";

        public static void RequirePermission(this IProvideMetadata type, Permission permission)
        {
            var permissions = type.GetMetadata<List<Permission>>(PermissionsKey);

            if (permissions is null)
            {
                type.Metadata[PermissionsKey] = permissions = new List<Permission>();
            }

            permissions.Add(permission);
        }

        public static FieldBuilder<TSourceType, TReturnType> RequirePermission<TSourceType, TReturnType>(
            this FieldBuilder<TSourceType, TReturnType> builder, Permission permission)
        {
            builder.FieldType.RequirePermission(permission);
            return builder;
        }
    }
}
