using System;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Security;

namespace OrchardCore.Users.EntityFrameworkCore.Models
{
    public class RoleClaim<TKey>: IdentityRoleClaim<TKey>
        where TKey : IEquatable<TKey>
    {
        
    }
}