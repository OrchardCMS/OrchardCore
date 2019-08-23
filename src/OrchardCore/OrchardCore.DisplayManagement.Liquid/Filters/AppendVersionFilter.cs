using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Filters
{
    public class AppendVersionFilter : ILiquidFilter
    {
        private readonly IFileVersionProvider _fileVersionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppendVersionFilter(
            IFileVersionProvider fileVersionProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            _fileVersionProvider = fileVersionProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var url = input.ToStringValue();

            var imageUrl = _fileVersionProvider.AddFileVersionToPath(_httpContextAccessor.HttpContext.Request.PathBase, url);

            return new ValueTask<FluidValue>(new StringValue(imageUrl ?? url));
        }
    }
}
