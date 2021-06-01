using OrchardCore.AuditTrail.Models;
using OrchardCore.DisplayManagement.Entities;

namespace OrchardCore.AuditTrail.Drivers
{
    /// <summary>
    /// A concrete implementation of this class will be able to take part in the rendering of an <see cref="AuditTrailEvent"/>
    /// shape instance for a specific section of the <see cref="AuditTrailEvent"/>. A section represents a property of an entity instance
    /// where the name of the property is the type of the section.
    /// </summary>
    /// <typeparam name="TSection">The type of the section this driver handles.</typeparam>
    public abstract class AuditTrailEventSectionDisplayDriver<TSection> : SectionDisplayDriver<AuditTrailEvent, TSection>
        where TSection : new()
    {
        public override bool CanHandleModel(AuditTrailEvent auditTrailEvent)
            => auditTrailEvent.Properties.ContainsKey(PropertyName);
    }
}
