using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.ContentManagement
{
    public class ContentPartBuilder
    {
        public ContentPartBuilder(IServiceCollection services, Type contentPartType)
        {
            Services = services;
            ContentPartType = contentPartType;
        }

        public IServiceCollection Services { get; }
        public Type ContentPartType { get; }
    }
}
