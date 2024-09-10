using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Users.Liquid;

public class UsersByIdFilter : ILiquidFilter
{
    private readonly ISession _session;

    public UsersByIdFilter(ISession session)
    {
        _session = session;
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

                var cachedUsers = await _session.GetAsync<User>(items.Keys.ToArray());

                var cachedUserIds = cachedUsers.Select(x => x.UserId).ToHashSet();

                var missingUserIds = items.Values.Where(userId => !cachedUserIds.Contains(userId)).ToList();

                var missingUsers = await _session.Query<User, UserIndex>(x => x.UserId.IsIn(missingUserIds)).ListAsync();

                return FluidValue.Create(missingUsers.Concat(cachedUsers).ToList(), ctx.Options);
            }

            return NilValue.Empty;
        }

        if (input.Type == FluidValues.Array)
        {
            // List of user ids
            var userIds = input.Enumerate(ctx).Select(x => x.ToStringValue()).ToArray();

            return FluidValue.Create(await _session.Query<User, UserIndex>(x => x.UserId.IsIn(userIds)).ListAsync(), ctx.Options);
        }

        var userId = input.ToStringValue();

        return FluidValue.Create(await _session.Query<User, UserIndex>(x => x.UserId == userId).FirstOrDefaultAsync(), ctx.Options);
    }
}
