using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Options;
using OrchardCore.Data;
using OrchardCore.Liquid;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Users.Liquid
{
    public class UsersByIdFilter : ILiquidFilter
    {
        private readonly ISession _session;
        private string userCollection = "User";
        IOptions<StoreCollectionOptions> _storeCollections;

        public UsersByIdFilter(ISession session, IOptions<StoreCollectionOptions> storeCollections)
        {
            _session = session;
            _storeCollections = storeCollections;
            userCollection = storeCollections.Value.Collections["OrchardCore.Users"] ?? "";
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
        {
            if (input.Type == FluidValues.Array)
            {
                // List of user ids
                var userIds = input.Enumerate().Select(x => x.ToStringValue()).ToArray();

                return FluidValue.Create(await _session.Query<User, UserIndex>(x => x.UserId.IsIn(userIds), userCollection).ListAsync(), ctx.Options);
            }

            var userId = input.ToStringValue();

            return FluidValue.Create(await _session.Query<User, UserIndex>(x => x.UserId == userId, userCollection).FirstOrDefaultAsync(), ctx.Options);
        }
    }
}
