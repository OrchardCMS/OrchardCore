using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Extensions.Localization;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailOptions
    {
        internal Dictionary<string, AuditTrailCategoryDescriptorBuilder> CategoryDescriptorBuilders { get; set; } = [];

        private ImmutableDictionary<string, AuditTrailCategoryDescriptor> _categoryDescriptors;
        public IReadOnlyDictionary<string, AuditTrailCategoryDescriptor> CategoryDescriptors
            => _categoryDescriptors ??= BuildCategoryDescriptors();

        private ImmutableDictionary<string, AuditTrailCategoryDescriptor> BuildCategoryDescriptors()
        {
            var categoryDescriptors = CategoryDescriptorBuilders.ToImmutableDictionary(k => k.Key, v => v.Value.Build());
            CategoryDescriptorBuilders = null;

            return categoryDescriptors;
        }
    }

    public static class AuditTrailOptionsExtensions
    {
        public static AuditTrailCategoryDescriptorBuilder For<TLocalizer>(this AuditTrailOptions options, string categoryName, Func<IStringLocalizer, LocalizedString> localizedName) where TLocalizer : class
        {
            if (!options.CategoryDescriptorBuilders.TryGetValue(categoryName, out var auditTrailCategoryDescriptorBuilder))
            {
                auditTrailCategoryDescriptorBuilder = new AuditTrailCategoryDescriptorBuilder<TLocalizer>(categoryName, localizedName);
                options.CategoryDescriptorBuilders[categoryName] = auditTrailCategoryDescriptorBuilder;
            }

            return auditTrailCategoryDescriptorBuilder;
        }

        public static bool Remove(this AuditTrailOptions options, string categoryName)
            => options.CategoryDescriptorBuilders.Remove(categoryName);
    }
}
