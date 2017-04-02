using System;
using System.Threading.Tasks;
using Orchard.Lucene;
using Orchard.Lucene.ViewModels;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Settings.Services;

namespace Orchard.Lucene.Drivers
{
    public class LuceneSiteSettingsDisplayDriver : SiteSettingsSectionDisplayDriver<LuceneSettings>
    {
        private readonly LuceneIndexProvider _luceneIndexProvider;

        public LuceneSiteSettingsDisplayDriver(LuceneIndexProvider luceneIndexProvider)
        {
            _luceneIndexProvider = luceneIndexProvider;
        }

        public override IDisplayResult Edit(LuceneSettings section, BuildEditorContext context)
        {
            return Shape<LuceneSettingsViewModel>("LuceneSettings_Edit", model =>
                {
                    model.SearchIndex = section.SearchIndex;
                    model.SearchFields = String.Join(", ", section.SearchFields ?? new string[0]);
                    model.SearchIndexes = _luceneIndexProvider.List();
                }).Location("Content:2").OnGroup("search");
        }

        public override async Task<IDisplayResult> UpdateAsync(LuceneSettings section, IUpdateModel updater, string groupId)
        {
            if (groupId == "search")
            {
                var model = new LuceneSettingsViewModel();

                await updater.TryUpdateModelAsync(model, Prefix);

                section.SearchIndex = model.SearchIndex;
                section.SearchFields = model.SearchFields?.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }

            return Edit(section);
        }
    }
}
