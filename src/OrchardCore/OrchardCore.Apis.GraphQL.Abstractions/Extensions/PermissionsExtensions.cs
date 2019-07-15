using System;
using System.Collections.Generic;
using GraphQL.Builders;
using GraphQL.Types;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Apis.GraphQL
{
    public static class PermissionsExtensions
    {
        public static readonly string PermissionsKey = "Permissions";

        public static void RequirePermission(this IProvideMetadata type, Permission permission, object resource = null)
        {
            var permissions = type.GetMetadata<List<Tuple<Permission, object>>>(PermissionsKey);

            if (permissions is null)
            {
                type.Metadata[PermissionsKey] = permissions = new List<Tuple<Permission, object>>();
            }

            permissions.Add(new Tuple<Permission, object>(permission, resource));
        }

        public static FieldBuilder<TSourceType, TReturnType> RequirePermission<TSourceType, TReturnType>(
            this FieldBuilder<TSourceType, TReturnType> builder, Permission permission, object resource = null)
        {
            builder.FieldType.RequirePermission(permission, resource);
            return builder;
        }
    }
}
