using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Security;

namespace OrchardCore.Users.EntityFrameworkCore.Models
{
    public class Role<TKey> : IdentityRole<TKey>, IRole where TKey : IEquatable<TKey>
    {
        [NotMapped]
        public string RoleName
        {
            get => base.Name;
            set => base.Name = value;
        }
    }
}