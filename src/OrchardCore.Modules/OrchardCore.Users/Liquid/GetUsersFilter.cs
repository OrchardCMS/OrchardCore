using System;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Users.Liquid
{
    public class UsersByIdFilter : ILiquidFilter
    {
        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!ctx.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'user'");
            }

            var session = ((IServiceProvider)services).GetRequiredService<ISession>();

            if (input.Type == FluidValues.Array)
            {
                // List of user ids
                var userIds = input.Enumerate().Select(x => x.ToStringValue()).ToArray();

                return FluidValue.Create(await (session.Query<User, UserIndex>(x => x.UserId.IsIn(userIds)).ListAsync()));
            }
            else
            {
                var userId = input.ToStringValue();

                return FluidValue.Create(await session.Query<User, UserIndex>(x => x.UserId == userId).FirstOrDefaultAsync());
            }
        }
    }
}
