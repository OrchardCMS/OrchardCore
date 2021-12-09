using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.ContentManagement
{
    public class ContentFieldOptionBuilder
    {
        public ContentFieldOptionBuilder(IServiceCollection services, Type contentFieldType)
        {
            Services = services;
            ContentFieldType = contentFieldType;
        }

        public IServiceCollection Services { get; }
        public Type ContentFieldType { get; }
    }
}
