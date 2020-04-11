using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.ContentManagement
{
    public class ContentPartOptionBuilder
    {
        public ContentPartOptionBuilder(IServiceCollection services, Type contentPartType)
        {
            Services = services;
            ContentPartType = contentPartType;
        }

        public IServiceCollection Services { get; }
        public Type ContentPartType { get; }
    }
}
