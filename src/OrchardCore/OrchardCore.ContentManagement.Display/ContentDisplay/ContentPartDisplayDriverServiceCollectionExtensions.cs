using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public static class ContentPartDisplayDriverServiceCollectionExtensions
    {
        public static ContentPartBuilder WithDisplayDriver<TDisplayDriver>(this ContentPartBuilder builder)
            where TDisplayDriver : class, IContentPartDisplayDriver
        {
            builder.Services.AddScoped<IContentPartDisplayDriver, TDisplayDriver>();
            return builder;
        }
    }
}