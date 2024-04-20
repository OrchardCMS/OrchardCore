using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.TimeZone.Services;

namespace OrchardCore.Users.TimeZone.Handlers;
public class TimeZoneUserEventHandler : UserEventHandlerBase
{
    private readonly UserTimeZoneService _userTimeZoneService;

    public TimeZoneUserEventHandler(UserTimeZoneService userTimeZoneService) => _userTimeZoneService = userTimeZoneService;

    public override async Task DeletedAsync(UserDeleteContext context) => await _userTimeZoneService.UpdateUserTimeZoneAsync(context.User);

    public override async Task UpdatedAsync(UserUpdateContext context) => await _userTimeZoneService.UpdateUserTimeZoneAsync(context.User);
}
