using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lucene.Model;
using OrchardCore.Lucene.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Lucene.Drivers
{
    public class LuceneSiteSettingsDisplayDriver : SectionDisplayDriver<ISite, LuceneSettings>
    {
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

        public LuceneSiteSettingsDisplayDriver(LuceneIndexSettingsService luceneIndexSettingsService)
        {
            _luceneIndexSettingsService = luceneIndexSettingsService;
        }

        public override IDisplayResult Edit(LuceneSettings section, BuildEditorContext context)
        {
            return Initialize<LuceneSettingsViewModel>("LuceneSettings_Edit", async model =>
                {
                    model.SearchIndex = section.SearchIndex;
                    model.SearchFields = String.Join(", ", section.DefaultSearchFields ?? new string[0]);
                    model.SearchIndexes = (await _luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName);
                }).Location("Content:2").OnGroup("search");
        }

        public override async Task<IDisplayResult> UpdateAsync(LuceneSettings section, BuildEditorContext context)
        {
            if (context.GroupId == "search")
            {
                var model = new LuceneSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                section.SearchIndex = model.SearchIndex;
                section.DefaultSearchFields = model.SearchFields?.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }

            return await EditAsync(section, context);
        }
    }
}
