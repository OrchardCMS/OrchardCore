using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace OrchardCore.Security.Permissions
{
    public class Permission
    {
        public const string ClaimType = "Permission";

        public Permission(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
        }

        public Permission(string name, string description, bool isSecurityCritical = false) : this(name)
        {
            Description = description;
            IsSecurityCritical = isSecurityCritical;
        }

        public Permission(string name, string description, IEnumerable<Permission> impliedBy, bool isSecurityCritical = false) : this(name, description, isSecurityCritical)
        {
            ImpliedBy = impliedBy;
        }

        public string Name { get; }
        public string Description { get; set; }
        public string Category { get; set; }
        public IEnumerable<Permission> ImpliedBy { get; }
        public bool IsSecurityCritical { get; }

        public static implicit operator Claim(Permission p)
        {
            return new Claim(ClaimType, p.Name);
        }
    }
}
