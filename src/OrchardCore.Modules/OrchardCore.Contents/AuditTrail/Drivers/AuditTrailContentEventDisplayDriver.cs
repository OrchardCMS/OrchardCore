using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.AuditTrail.Drivers;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.Contents.AuditTrail.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using YesSql;

namespace OrchardCore.Contents.AuditTrail.Drivers
{
    public class AuditTrailContentEventDisplayDriver : AuditTrailEventSectionDisplayDriver<AuditTrailContentEvent>
    {
        private readonly Dictionary<string, string> _latestVersionId = new();
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly ISession _session;

        public AuditTrailContentEventDisplayDriver(IAuditTrailManager auditTrailManager, ISession session)
        {
            _auditTrailManager = auditTrailManager;
            _session = session;
        }

        public override async Task<IDisplayResult> DisplayAsync(AuditTrailEvent auditTrailEvent, AuditTrailContentEvent contentEvent, BuildDisplayContext context)
        {
            var contentItemId = contentEvent.ContentItem.ContentItemId;

            if (!_latestVersionId.TryGetValue(contentItemId, out var latestVersionId))
            {
                latestVersionId = (await _session.QueryIndex<ContentItemIndex>(index => index.ContentItemId == contentItemId && index.Latest)
                    .FirstOrDefaultAsync())
                    ?.ContentItemVersionId;

                _latestVersionId[contentItemId] = latestVersionId;
            }


            var descriptor = _auditTrailManager.DescribeEvent(auditTrailEvent);

            return Combine(
                Initialize<AuditTrailContentEventViewModel>("AuditTrailContentEventData_SummaryAdmin", m => BuildSummaryViewModel(m, auditTrailEvent, contentEvent, descriptor, latestVersionId))
                    .Location("SummaryAdmin", "EventData:10"),
                Initialize<AuditTrailContentEventViewModel>("AuditTrailContentEventContent_SummaryAdmin", m => BuildSummaryViewModel(m, auditTrailEvent, contentEvent, descriptor, latestVersionId))
                    .Location("SummaryAdmin", "Content:10"),
                Initialize<AuditTrailContentEventViewModel>("AuditTrailContentEventActions_SummaryAdmin", m => BuildSummaryViewModel(m, auditTrailEvent, contentEvent, descriptor, latestVersionId))
                    .Location("SummaryAdmin", "Actions:5"),
                Initialize<AuditTrailContentEventDetailViewModel>("AuditTrailContentEventDetail_DetailAdmin", async m =>
                {
                    BuildSummaryViewModel(m, auditTrailEvent, contentEvent, descriptor, latestVersionId);
                    var previousContentItem = (await _session.Query<AuditTrailEvent, AuditTrailEventIndex>(collection: AuditTrailEvent.Collection)
                        .Where(index =>
                            index.Category == "Content" &&
                            index.CreatedUtc <= auditTrailEvent.CreatedUtc &&
                            index.EventId != auditTrailEvent.EventId &&
                            index.CorrelationId == contentEvent.ContentItem.ContentItemId)
                        .OrderByDescending(index => index.Id)
                        .FirstOrDefaultAsync())?
                        .As<AuditTrailContentEvent>()
                        .ContentItem;

                    if (previousContentItem != null)
                    {
                        var current = JObject.FromObject(contentEvent.ContentItem);
                        var previous = JObject.FromObject(previousContentItem);
                        previous.Remove(nameof(AuditTrailPart));
                        current.Remove(nameof(AuditTrailPart));

                        m.PreviousContentItem = previousContentItem;

                        m.Previous = previous.ToString();
                        m.Current = current.ToString();
                    }
                }).Location("DetailAdmin", "Content:5"),
                Initialize<AuditTrailContentEventDetailViewModel>("AuditTrailContentEventDiff_DetailAdmin", async m =>
                {
                    BuildSummaryViewModel(m, auditTrailEvent, contentEvent, descriptor, latestVersionId);
                    var previousContentItem = (await _session.Query<AuditTrailEvent, AuditTrailEventIndex>(collection: AuditTrailEvent.Collection)
                        .Where(index =>
                            index.Category == "Content" &&
                            index.CreatedUtc <= auditTrailEvent.CreatedUtc &&
                            index.EventId != auditTrailEvent.EventId &&
                            index.CorrelationId == contentEvent.ContentItem.ContentItemId)
                        .OrderByDescending(index => index.Id)
                        .FirstOrDefaultAsync())?
                        .As<AuditTrailContentEvent>()
                        .ContentItem;

                    if (previousContentItem != null)
                    {
                        var current = JObject.FromObject(contentEvent.ContentItem);
                        var previous = JObject.FromObject(previousContentItem);
                        previous.Remove(nameof(AuditTrailPart));
                        current.Remove(nameof(AuditTrailPart));

                        m.PreviousContentItem = previousContentItem;

                        m.Previous = previous.ToString();
                        m.Current = current.ToString();
                    }
                }).Location("DetailAdmin", "Content:5#Diff")
            );
        }

        private static void BuildSummaryViewModel(AuditTrailContentEventViewModel m, AuditTrailEvent model, AuditTrailContentEvent contentEvent, AuditTrailEventDescriptor descriptor, string latestVersionId)
        {
            m.AuditTrailEvent = model;
            m.Descriptor = descriptor;
            m.Name = contentEvent.Name;
            m.ContentItem = contentEvent.ContentItem;
            m.VersionNumber = contentEvent.VersionNumber;
            m.Comment = contentEvent.Comment;
            m.LatestVersionId = latestVersionId;
            m.ContentEvent = contentEvent;
        }
    }
}
