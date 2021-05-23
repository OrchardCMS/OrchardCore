using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Models;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.Contents.AuditTrail.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Contents.AuditTrail.Providers;
using YesSql;
using OrchardCore.ContentManagement.Records;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.Indexes;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;

namespace OrchardCore.Contents.AuditTrail.Drivers
{
    public class AuditTrailContentEventDisplayDriver : SectionDisplayDriver<AuditTrailEvent, AuditTrailContentEvent>
    {
        private readonly ISession _session;
        public AuditTrailContentEventDisplayDriver(ISession session)
        {
            _session = session;
        }

        private readonly Dictionary<string, string> _latestVersionId = new Dictionary<string, string>();

        public override async Task<IDisplayResult> DisplayAsync(AuditTrailEvent auditTrailEvent, AuditTrailContentEvent contentEvent, BuildDisplayContext context)
        {
            if (!auditTrailEvent.Properties.ContainsKey(PropertyName))
            {
                return null;
            }

            var contentItemId = contentEvent.ContentItem.ContentItemId;

                if (!_latestVersionId.TryGetValue(contentItemId, out var latestVersionId))
                {
                    latestVersionId = (await _session.QueryIndex<ContentItemIndex>(index => index.ContentItemId == contentItemId && index.Latest)
                        .FirstOrDefaultAsync())
                        ?.ContentItemVersionId;

                    _latestVersionId[contentItemId] = latestVersionId;
                }

            return Combine(
                Initialize<AuditTrailContentEventViewModel>("AuditTrailContentEventHeader_SummaryAdmin", m => BuildSummaryViewModel(m, auditTrailEvent, contentEvent, latestVersionId))
                    .Location("SummaryAdmin", "Header:10"),
                Initialize<AuditTrailContentEventViewModel>("AuditTrailContentEventEventData_SummaryAdmin", m => BuildSummaryViewModel(m, auditTrailEvent, contentEvent, latestVersionId))
                        .Location("SummaryAdmin","EventData:10"),
                Initialize<AuditTrailContentEventViewModel>("AuditTrailContentEventActions_SummaryAdmin", m => BuildSummaryViewModel(m, auditTrailEvent, contentEvent, latestVersionId))
                        .Location("SummaryAdmin","Actions:5"),
                Initialize<AuditTrailContentEventDetailViewModel>("AuditTrailContentEventDetail_DetailAdmin", async m =>
                {
                    BuildSummaryViewModel(m, auditTrailEvent, contentEvent, latestVersionId);
                    m.DiffNodes = await BuildDiffNodesAsync(auditTrailEvent, contentEvent);
                }).Location("DetailAdmin","Content:5")
            );
        }

        private async Task<DiffNode[]> BuildDiffNodesAsync(AuditTrailEvent auditTrailEvent, AuditTrailContentEvent contentEvent)
        {
            var contentItem = contentEvent.ContentItem;

            var previousAuditTrailEvent = await _session.Query<AuditTrailEvent, AuditTrailEventIndex>(collection: AuditTrailEvent.Collection)
                .Where(index =>
                    index.Category == "Content" &&
                    index.CreatedUtc <= auditTrailEvent.CreatedUtc &&
                    index.EventId != auditTrailEvent.EventId &&
                    index.CorrelationId == contentItem.ContentItemId)
                .OrderByDescending(index => index.Id)
                .FirstOrDefaultAsync();

            if (previousAuditTrailEvent == null)
            {
                return null;
            }

            var previousContentItem = previousAuditTrailEvent.As<AuditTrailContentEvent>().ContentItem;

            var current = JObject.FromObject(contentItem);
            var previous = JObject.FromObject(previousContentItem);
            previous.Remove(nameof(AuditTrailPart));
            current.Remove(nameof(AuditTrailPart));

            if (current.FindDiff(previous, out var diff))
            {
                return diff.GenerateDiffNodes(contentItem.ContentType);
            }

            return null;
        }


        private static void BuildSummaryViewModel(AuditTrailContentEventViewModel m, AuditTrailEvent model, AuditTrailContentEvent contentEvent, string latestVersionId)
        {
            m.AuditTrailEvent = model;
            m.Name = contentEvent.Name;
            m.ContentItem = contentEvent.ContentItem;
            m.VersionNumber = contentEvent.VersionNumber;
            m.LatestVersionId = latestVersionId;
            m.ContentEvent = contentEvent;
        }
    }
}
