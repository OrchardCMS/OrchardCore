using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentLocalization.Handlers;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;
using YesSql;

namespace OrchardCore.Lists.Drivers
{
    public class ListPartLocalizationHandler : ContentLocalizationPartHandlerBase<ListPart>
    {
        private readonly ISession _session;

        public ListPartLocalizationHandler(ISession session)
        {
            _session = session;
        }

        /// <summary>
        /// Select Contained ContentItems that are already in the target culture
        /// but attached to the original list and reassign their ListContenItemId.
        /// </summary>
        public override async Task LocalizedAsync(LocalizationContentContext context, ListPart part)
        {
            var containedList = await _session.Query<ContentItem, ContainedPartIndex>(
                x => x.ListContentItemId == context.Original.ContentItemId).ListAsync();

            if (!containedList.Any())
            {
                return;
            }

            foreach (var item in containedList)
            {
                var localizationPart = item.As<LocalizationPart>();
                if (localizationPart.Culture == context.Culture)
                {
                    var cp = item.As<ContainedPart>();
                    cp.ListContentItemId = context.ContentItem.ContentItemId;
                    cp.Apply();
                    _session.Save(item);
                }
            }
        }
    }
}
