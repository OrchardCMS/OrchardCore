using System;
using Microsoft.AspNetCore.Identity;

namespace OrchardCore.Users.EntityFrameworkCore.Models
{
    public class User<TKey> : IdentityUser<TKey>, IUser
        where TKey : IEquatable<TKey>
        
    {
    }
    
}