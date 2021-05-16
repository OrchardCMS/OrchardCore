using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Models;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class DescribeFor
    {
        private readonly IList<AuditTrailEventDescriptor> _events = new List<AuditTrailEventDescriptor>();

        public DescribeFor(string category, string categoryFullName, string providerName, LocalizedString localizedName)
        {
            Category = category;
            CategoryFullName = categoryFullName;
            ProviderName = providerName;
            LocalizedName = localizedName;
        }

        public string Category { get; private set; }
        public string CategoryFullName { get; private set; }
        public string ProviderName { get; set; }
        public LocalizedString LocalizedName { get; private set; }
        public IEnumerable<AuditTrailEventDescriptor> Events => _events;

        public DescribeFor Event(
            string name,
            LocalizedString localizedName,
            LocalizedString description,
            Action<AuditTrailEvent, Dictionary<string, object>> buildEvent,
            bool enableByDefault = false,
            bool isMandatory = false)
        {
            _events.Add(new AuditTrailEventDescriptor
            {
                Name = name,
                FullName = CategoryFullName + name,
                LocalizedName = localizedName,
                Description = description,

                Category = new AuditTrailCategoryDescriptor
                {
                    Name = Category,
                    FullName = CategoryFullName,
                    LocalizedName = LocalizedName,
                    Events = Events
                },

                BuildEvent = buildEvent,
                IsEnabledByDefault = enableByDefault,
                IsMandatory = isMandatory
            });

            return this;
        }
    }
}
