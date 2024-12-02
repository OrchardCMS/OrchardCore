
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OrchardCore.Modules;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Handlers;

internal sealed class DefaultUserEventHandler : UserEventHandlerBase
{
    private readonly IClock _clock;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DefaultUserEventHandler(
        IClock clock,
        IHttpContextAccessor httpContextAccessor)
    {
        _clock = clock;
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task CreatingAsync(UserCreateContext context)
    {
        if (context.User is User user)
        {
            user.CreatedUtc = _clock.UtcNow;

            if (_httpContextAccessor.HttpContext?.User is not null)
            {
                user.CreatedById = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
        }

        return Task.CompletedTask;
    }

    public override Task UpdatingAsync(UserUpdateContext context)
    {
        if (context.User is User u)
        {
            u.ModifiedUtc = _clock.UtcNow;

            if (_httpContextAccessor.HttpContext?.User is not null)
            {
                u.ModifiedById = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
        }

        return Task.CompletedTask;
    }
}
