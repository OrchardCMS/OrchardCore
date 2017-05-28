using Orchard.Security.Permissions;
using System.Collections.Generic;

namespace Orchard.Queries.Sql
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageSqlQueries = new Permission("ManageSqlQueries", "Manage SQL Queries");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] 
            {
                ManageSqlQueries
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageSqlQueries }
                },
                new PermissionStereotype
                {
                    Name = "Editor",
                    Permissions = new[] { ManageSqlQueries }
                }
            };
        }
    }
}