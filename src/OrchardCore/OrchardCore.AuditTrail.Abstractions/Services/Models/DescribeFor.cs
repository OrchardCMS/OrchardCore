using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Models;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class DescribeFor
    {
        private readonly HashSet<string> _names = new HashSet<string>();
        private readonly IList<AuditTrailEventDescriptor> _events = new List<AuditTrailEventDescriptor>();

        public DescribeFor(string category, LocalizedString localizedName)
        {
            Category = category;
            LocalizedName = localizedName;
        }

        public string Category { get; private set; }
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
            if (_names.Add(name))
            {
                _events.Add(new AuditTrailEventDescriptor
                {
                    Name = name,
                    Category = Category,
                    LocalizedName = localizedName,
                    LocalizedCategory = LocalizedName,
                    Description = description,
                    BuildEvent = buildEvent,
                    IsEnabledByDefault = enableByDefault,
                    IsMandatory = isMandatory
                });
            }

            return this;
        }
    }
}
