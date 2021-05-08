using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace OrchardCore.Tests.Modules.OrchardCore.Roles
{
    public static class RolesMockHelper
    {
        public static Mock<RoleManager<TRole>> MockRoleManager<TRole>()
            where TRole : class
        {
            var store = new Mock<IRoleStore<TRole>>().Object;
            var validators = new List<IRoleValidator<TRole>>();
            validators.Add(new RoleValidator<TRole>());

            return new Mock<RoleManager<TRole>>(store, validators, new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null);
        }
    }
}
