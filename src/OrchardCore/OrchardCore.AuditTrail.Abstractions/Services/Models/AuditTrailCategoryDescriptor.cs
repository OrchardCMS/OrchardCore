using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailCategoryDescriptor
    {
        public AuditTrailCategoryDescriptor(
            string name,
            Func<IServiceProvider, LocalizedString> localizedName,
            IReadOnlyDictionary<string, AuditTrailEventDescriptor> events)
        {
            Name = name;
            LocalizedName = localizedName;
            Events = events;
        }

        public string Name { get; }
        public Func<IServiceProvider, LocalizedString> LocalizedName { get; }
        public IReadOnlyDictionary<string, AuditTrailEventDescriptor> Events { get; }

        private static readonly IReadOnlyDictionary<string, AuditTrailEventDescriptor> _empty =
            new Dictionary<string, AuditTrailEventDescriptor>();

        public static AuditTrailCategoryDescriptor Default(string name) =>
            new(
                name,
                (sp) => new LocalizedString(name, name),
                _empty
            );
    }
}
