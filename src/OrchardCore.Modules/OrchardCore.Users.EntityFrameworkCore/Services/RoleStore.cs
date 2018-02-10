using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrchardCore.Security;
using OrchardCore.Users.EntityFrameworkCore.Models;

namespace OrchardCore.Users.EntityFrameworkCore.Services
{
    public class RoleStore<TRole, TContext, TKey, TUserRole, TRoleClaim> :
        Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<TRole, TContext, TKey, TUserRole, TRoleClaim>,
        IRoleClaimStore<IRole>
        where TRole : Role<TKey>, new()
        where TKey : IEquatable<TKey>
        where TContext : DbContext
        where TUserRole : IdentityUserRole<TKey>, new()
        where TRoleClaim : RoleClaim<TKey>, new()
    {
        public RoleStore(TContext context, IdentityErrorDescriber describer = null) : base(context, describer)
        {
            
        }

        #region Store for IRole implementation
        public async Task<IdentityResult> CreateAsync(IRole role, CancellationToken cancellationToken)
        {
            var storeRole = ConvertToStoreRoleAsync(role);
            var result = await base.CreateAsync(storeRole, cancellationToken);
            if (result == IdentityResult.Success)
                await SyncClaimsAsync(storeRole.NormalizedName, role);
            return result;
        }

        public async Task<IdentityResult> UpdateAsync(IRole role, CancellationToken cancellationToken)
        {
            var storeRole = ConvertToStoreRoleAsync(role);
            if (role is Role orchardRole)
            {
                var currentStoreRole = await base.FindByNameAsync(orchardRole.NormalizedRoleName, cancellationToken);
                if (currentStoreRole != null)
                {
                    currentStoreRole.NormalizedName = orchardRole.NormalizedRoleName;
                    currentStoreRole.RoleName = orchardRole.RoleName;
                    storeRole = currentStoreRole;
                }
            }
            var result = await base.UpdateAsync(storeRole, cancellationToken);
            return result;
        }

        public Task<IdentityResult> DeleteAsync(IRole role, CancellationToken cancellationToken)
        {
            var storeRole = ConvertToStoreRoleAsync(role);
            return base.DeleteAsync(storeRole, cancellationToken);
        }

        public Task<string> GetRoleIdAsync(IRole role, CancellationToken cancellationToken)
        {
            var storeRole = ConvertToStoreRoleAsync(role);
            return base.GetRoleIdAsync(storeRole, cancellationToken);
        }

        public Task<string> GetRoleNameAsync(IRole role, CancellationToken cancellationToken)
        {
            var storeRole = ConvertToStoreRoleAsync(role);
            return base.GetRoleNameAsync(storeRole, cancellationToken);
        }

        public async Task SetRoleNameAsync(IRole role, string roleName, CancellationToken cancellationToken)
        {
            var storeRole = ConvertToStoreRoleAsync(role);
            await base.SetRoleNameAsync(storeRole, roleName, cancellationToken);
            SyncChangesAsync(storeRole, role);
        }

        public Task<string> GetNormalizedRoleNameAsync(IRole role, CancellationToken cancellationToken)
        {
            var storeRole = ConvertToStoreRoleAsync(role);
            return base.GetNormalizedRoleNameAsync(storeRole, cancellationToken);
        }

        public async Task SetNormalizedRoleNameAsync(IRole role, string normalizedName, CancellationToken cancellationToken)
        {
            var storeRole = ConvertToStoreRoleAsync(role);
            await base.SetNormalizedRoleNameAsync(storeRole, normalizedName, cancellationToken);
            SyncChangesAsync(storeRole, role);
        }

        async Task<IRole> IRoleStore<IRole>.FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            var role = await base.FindByIdAsync(roleId, cancellationToken);
            return await ConvertToRoleAsync(role);
        }

        async Task<IRole> IRoleStore<IRole>.FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            var role = await base.FindByNameAsync(normalizedRoleName, cancellationToken);
            return await ConvertToRoleAsync(role);
        }

        public Task<IList<Claim>> GetClaimsAsync(IRole role, CancellationToken cancellationToken = new CancellationToken())
        {
            var storeRole = ConvertToStoreRoleAsync(role);
            return base.GetClaimsAsync(storeRole, cancellationToken);
        }

        public async Task AddClaimAsync(IRole role, Claim claim, CancellationToken cancellationToken = new CancellationToken())
        {
            if (role is Role orchardRole)
            {
                var currentStoreRole = await base.FindByNameAsync(orchardRole.NormalizedRoleName, cancellationToken);
                if (currentStoreRole != null)
                {
                    await base.AddClaimAsync(currentStoreRole, claim, cancellationToken);
                    SyncChangesAsync(currentStoreRole, role);
                }
            }
        }

        public async Task RemoveClaimAsync(IRole role, Claim claim, CancellationToken cancellationToken = new CancellationToken())
        {
            var storeRole = ConvertToStoreRoleAsync(role);
            await base.RemoveClaimAsync(storeRole, claim, cancellationToken);
        }


        /// <summary>
        /// Convert to Store Role
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        protected virtual TRole ConvertToStoreRoleAsync(IRole role)
        {
            if (role == null)
                return null;
            if (role is Role orchardRole)
            {
                return new TRole
                {
                    Name = orchardRole.RoleName,
                    NormalizedName = orchardRole.NormalizedRoleName
                };
            }

            return role is TRole identityRole
                ? identityRole
                : new TRole
                {
                    Name = role.RoleName
                };
        }


        /// <summary>
        /// Convert to Application Role
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        protected virtual async Task<IRole> ConvertToRoleAsync(TRole role)
        {
            if (role == null)
                return null;
            var claims = await this.GetClaimsAsync(role);

            var newRole = new Role
            {
                RoleName = role.RoleName,
                NormalizedRoleName = role.NormalizedName,
            };
            newRole.RoleClaims.AddRange(claims.Select(claim => new RoleClaim { ClaimType = claim.Type, ClaimValue = claim.Value }));
            return newRole;
        }


        /// <summary>
        /// Sync changes from source to destination
        /// </summary>
        /// <param name="source">Source Role</param>
        /// <param name="destination">Destination Role</param>
        protected virtual async void SyncChangesAsync(TRole source, IRole destination)
        {
            if (source == null || destination == null)
                return;
            if (destination is Role orchardRole)
            {
                orchardRole.RoleName = source.RoleName;
                orchardRole.NormalizedRoleName = source.NormalizedName;
                var claims = await GetClaimsAsync(source);
                orchardRole.RoleClaims.AddRange(claims.Select(claim => new RoleClaim() { ClaimType = claim.Type, ClaimValue = claim.Value }));
            }
        }

        /// <summary>
        /// Sync Claims back to DB
        /// </summary>
        /// <param name="normalizedName"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        protected virtual async Task SyncClaimsAsync(string normalizedName, IRole role)
        {
            if (role is Role orchardRole)
            {
                TRole target = await base.FindByNameAsync(normalizedName);
                if (target == null)
                    return;
                var originalClaims = await base.GetClaimsAsync(target);
                if (originalClaims.Count != orchardRole.RoleClaims.Count || orchardRole.RoleClaims.Any(claim => originalClaims.FirstOrDefault(oClaim => oClaim.Type == claim.ClaimType) == null))
                {
                    foreach (var originalClaim in originalClaims)
                    {
                        await base.RemoveClaimAsync(target, originalClaim);
                    }
                    foreach (var roleClaim in orchardRole.RoleClaims)
                    {
                        await base.AddClaimAsync(target, new Claim(roleClaim.ClaimType, roleClaim.ClaimType));
                    }
                }
            }
        } 
        #endregion
    }
}