using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Liquid;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Users.Liquid;

public class UsersByNameFilter : ILiquidFilter
{
    private readonly ISession _session;
    private readonly UserManager<IUser> _userManager;

    public UsersByNameFilter(ISession session, UserManager<IUser> userManager)
    {
        _session = session;
        _userManager = userManager;
    }

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
    {
        if (input.Type == FluidValues.Dictionary)
        {
            if (input.ToObjectValue() is IFluidIndexable values)
            {
                var items = new Dictionary<long, string>();

                foreach (var key in values.Keys)
                {
                    if (long.TryParse(key, out var id) && values.TryGetValue(key, out var value))
                    {
                        items.Add(id, value.ToStringValue());
                    }
                }

                var normalizedUserNames = items.Values.Select(userName => _userManager.NormalizeName(userName)).ToArray();

                var cachedUsers = await _session.GetAsync<User>(items.Keys.ToArray());

                var cachedNormalizedNames = cachedUsers.Select(x => x.NormalizedUserName).ToHashSet();

                var missingNormalizedNames = normalizedUserNames.Where(name => !cachedNormalizedNames.Contains(name)).ToList();

                var missingUsers = await _session.Query<User, UserIndex>(x => x.NormalizedUserName.IsIn(missingNormalizedNames)).ListAsync();

                return FluidValue.Create(missingUsers.Concat(cachedUsers).ToList(), ctx.Options);
            }

            return EmptyValue.Instance;
        }

        if (input.Type == FluidValues.Array)
        {
            // List of usernames
            var userNames = input.Enumerate(ctx).Select(x => x.ToStringValue()).ToArray();
            var normalizedUserNames = userNames.Select(userName => _userManager.NormalizeName(userName)).ToArray();

            return FluidValue.Create(await _session.Query<User, UserIndex>(x => x.NormalizedUserName.IsIn(normalizedUserNames)).ListAsync(), ctx.Options);
        }

        var userName = input.ToStringValue();
        var normalizedUserName = _userManager.NormalizeName(userName);

        return FluidValue.Create(await _session.Query<User, UserIndex>(x => x.NormalizedUserName == normalizedUserName).FirstOrDefaultAsync(), ctx.Options);
    }
}
