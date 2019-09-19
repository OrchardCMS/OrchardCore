using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.ContentManagement
{
    public class ContentPartBuilder
    {
        public ContentPartBuilder(IServiceCollection services)
            => Services = services;

        public virtual IServiceCollection Services { get; }
    }
}