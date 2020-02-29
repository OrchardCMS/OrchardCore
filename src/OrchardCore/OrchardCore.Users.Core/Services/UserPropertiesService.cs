using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services
{
    public abstract class UserPropertiesService<T> where T : new()
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IUser> _userManager;
        public UserPropertiesService(
            IHttpContextAccessor httpContextAccessor,
            UserManager<IUser> userManager
            )
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public IQueryable<IUser> GetUsers()
        {
            var users = _userManager.Users;
            return users;
        }
        public async Task<User> GetCurrentUserAsync()
        {
            var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

            if (String.IsNullOrEmpty(userName))
            {
                return null;
            }
            var user = await _userManager.FindByNameAsync(userName) as User;
            return user;
        }
        public async Task<T> GetCurrentUserPropertiesAsync()
        {
            var user = await GetCurrentUserAsync();
            return GetUserPropertiesAsync(user);
        }

        public T GetUserPropertiesAsync(User user)
        {

            if (user != null)
            {
                return user.As<T>();
            }
            else
            {
                return default(T);
            }

        }

        public async Task UpdateCurrentUserPropertiesWith(T t)
        {
            var user = await GetCurrentUserAsync();
            await UpdateUserPropertiesWith(user, t);
        }
        public async Task UpdateUserPropertiesWith(User user, T t)
        {
            if (user != null)
            {
                user.Properties[nameof(T)] = JObject.FromObject(t);
                await _userManager.UpdateAsync(user);
            }
        }
    }
}



