using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.ContentManagement
{
    public static class ContentPartServiceCollectionExtensions
    {
        public static ContentPartBuilder AddContentPart<TContentPart>(this IServiceCollection services)        
            where TContentPart : ContentPart
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Configure<ContentPartOptions>(options =>
            {
                options.AddPart<TContentPart>();
            });

            return new ContentPartBuilder(services);
        }
    }
}