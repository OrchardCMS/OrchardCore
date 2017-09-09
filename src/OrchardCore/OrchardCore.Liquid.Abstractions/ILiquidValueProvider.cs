using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Liquid
{
    public interface ILiquidValueProvider
    {
        Task PopulateValuesAsync(IDictionary<string, object> values);
    }
}
