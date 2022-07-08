using System;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Models;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailEventDescriptor
    {
        public AuditTrailEventDescriptor(string name, string category, Func<IServiceProvider, LocalizedString> localizedName, Func<IServiceProvider, LocalizedString> localizedCategory, Func<IServiceProvider, LocalizedString> description, bool isEnabledByDefault = false, bool isMandatory = false)
        {
            Name = name;
            Category = category;
            LocalizedName = localizedName;
            LocalizedCategory = localizedCategory;
            Description = description;
            IsEnabledByDefault = isEnabledByDefault;
            IsMandatory = isMandatory;
        }

        public string Name { get; }
        public string Category { get; }
        public Func<IServiceProvider, LocalizedString> LocalizedName { get; }
        public Func<IServiceProvider, LocalizedString> LocalizedCategory { get; }
        public Func<IServiceProvider, LocalizedString> Description { get; }
        public bool IsEnabledByDefault { get; }
        public bool IsMandatory { get; }

        public static AuditTrailEventDescriptor Default(AuditTrailEvent auditTrailEvent)
            => new AuditTrailEventDescriptor(
                    auditTrailEvent.Name,
                    auditTrailEvent.Category,
                    (sp) => new LocalizedString(auditTrailEvent.Name, auditTrailEvent.Name),
                    (sp) => new LocalizedString(auditTrailEvent.Category, auditTrailEvent.Category),
                    (sp) => new LocalizedString(auditTrailEvent.Name, auditTrailEvent.Name)
                );
    }
}
