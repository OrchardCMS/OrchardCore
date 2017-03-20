using System;
using System.Collections.Generic;
using OrchardCore.Tenant.Descriptor.Models;
using OrchardCore.Extensions.Features;

namespace OrchardCore.Tenant.Builders.Models
{
    /// <summary>
    /// Contains the information necessary to initialize an IoC container
    /// for a particular tenant. This model is created by the ICompositionStrategy
    /// and is passed into the ITenantContainerFactory.
    /// </summary>
    public class TenantBlueprint
    {
        public TenantSettings Settings { get; set; }
        public TenantDescriptor Descriptor { get; set; }

        public IDictionary<Type, FeatureEntry> Dependencies { get; set; }
    }
}