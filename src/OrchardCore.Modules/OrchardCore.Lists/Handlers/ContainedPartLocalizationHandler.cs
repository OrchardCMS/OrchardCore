using System.Threading.Tasks;
using OrchardCore.ContentLocalization.Handlers;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Lists.Models;
using YesSql;

namespace OrchardCore.Lists.Drivers
{
    public class ContainedPartLocalizationHandler : ContentLocalizationPartHandlerBase<LocalizationPart>
    {
        private readonly ISession _session;

        public ContainedPartLocalizationHandler(ISession session)
        {
            _session = session;
        }

        /// <summary>
        /// Assign the Localized version of the List when localizing the Contained Item
        /// </summary>
        public override async Task LocalizingAsync(LocalizationContentContext context, LocalizationPart part)
        {
            var containedPart = context.ContentItem.As<ContainedPart>();
            // todo: remove this check and change the handler to target ContainedPart when issue 3890 is fixed
            if (containedPart != null)
            {
                var list = await _session.QueryIndex<LocalizedContentItemIndex>(i => (i.Published || i.Latest) && i.ContentItemId == containedPart.ListContentItemId).FirstOrDefaultAsync();
                var localizedList = await _session.QueryIndex<LocalizedContentItemIndex>(i => (i.Published || i.Latest) && i.LocalizationSet == list.LocalizationSet && i.Culture == context.Culture).FirstOrDefaultAsync();

                if (localizedList != null)
                {
                    containedPart.ListContentItemId = localizedList.ContentItemId;
                    containedPart.Apply();
                }
            }
        }
    }

    public class LocalizationContainedPartHandler : ContentPartHandler<LocalizationPart>
    {
        private readonly ISession _session;
        public LocalizationContainedPartHandler(ISession session)
        {
            _session = session;
        }

        /// <summary>
        /// Need to override CreatingAsync to set the right parent on Creation.
        /// This will attach the item to the right list when the item is created from a list of another culture
        /// </summary>
        public override async Task CreatingAsync(CreateContentContext context, LocalizationPart instance)
        {
            var containedPart = context.ContentItem.As<ContainedPart>();
            if (containedPart != null)
            {
                var list = await _session.QueryIndex<LocalizedContentItemIndex>(i => (i.Published || i.Latest) && i.ContentItemId == containedPart.ListContentItemId).FirstOrDefaultAsync();
                var localizedList = await _session.QueryIndex<LocalizedContentItemIndex>(i => (i.Published || i.Latest) && i.LocalizationSet == list.LocalizationSet && i.Culture == instance.Culture).FirstOrDefaultAsync();

                if (localizedList != null)
                {
                    containedPart.ListContentItemId = localizedList.ContentItemId;
                    containedPart.Apply();
                }
            }
        }
    }
}
