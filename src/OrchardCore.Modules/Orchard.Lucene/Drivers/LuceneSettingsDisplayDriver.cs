using System;
using System.Threading.Tasks;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Entities.DisplayManagement;
using Orchard.Lucene.ViewModels;
using Orchard.Settings;

namespace Orchard.Lucene.Drivers
{
    public class LuceneSiteSettingsDisplayDriver : SectionDisplayDriver<ISite, LuceneSettings>
    {
        private readonly LuceneIndexManager _luceneIndexProvider;

        public LuceneSiteSettingsDisplayDriver(LuceneIndexManager luceneIndexProvider)
        {
            _luceneIndexProvider = luceneIndexProvider;
        }

        public override IDisplayResult Edit(LuceneSettings section, BuildEditorContext context)
        {
            return Shape<LuceneSettingsViewModel>("LuceneSettings_Edit", model =>
                {
                    model.SearchIndex = section.SearchIndex;
                    model.SearchFields = String.Join(", ", section.DefaultSearchFields ?? new string[0]);
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
                section.DefaultSearchFields = model.SearchFields?.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }

            return Edit(section);
        }
    }
}
