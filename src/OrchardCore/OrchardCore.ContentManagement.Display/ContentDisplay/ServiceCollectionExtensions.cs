using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public static class ServiceCollectionExtensions
    {
        public static ContentPartBuilder WithDisplayDriver<TContentPartDisplayDriver>(this ContentPartBuilder contentPartBuilder)
            where TContentPartDisplayDriver : class, IContentPartDisplayDriver
        {
            // For backward compatability, probably not required.
            //contentPartBuilder.Services.AddScoped<IContentPartDisplayDriver>(sp => sp.GetRequiredService<TContentPartDisplayDriver>());

            var displayDriverType = typeof(TContentPartDisplayDriver);

            if (displayDriverType.BaseType != null && !Array.Exists(displayDriverType.BaseType.GenericTypeArguments, x => x == contentPartBuilder.ContentPartType))
            {
                throw new ArgumentException("The display driver type must inherit from " + contentPartBuilder.ContentPartType.Name);
            }

            contentPartBuilder.Services.AddScoped<TContentPartDisplayDriver>();
            contentPartBuilder.Services.Configure<ContentOptions>(o =>
                o.WithContentPartFactoryType("displaydriver", contentPartBuilder.ContentPartType, displayDriverType));
            return contentPartBuilder;
        }
    }
}
