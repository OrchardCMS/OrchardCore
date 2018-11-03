using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lucene.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Lucene.Drivers
{
    public class LuceneAnalyzerSettingsDisplayerDriver : SectionDisplayDriver<ISite, LuceneAnalyzerSettings>
    {
       
        public LuceneAnalyzerSettingsDisplayerDriver()
        {           
        }

        public override IDisplayResult Edit(LuceneAnalyzerSettings section, BuildEditorContext context)
        {
            return Initialize<LuceneSettingsViewModel>("LuceneAnalyzerSettings_Edit", model =>
            {
                model.Analyzer = section.Analyzer;
                model.Version = section.Version;
               
            }).Location("Content:3").OnGroup("search");
        }

        public override async Task<IDisplayResult> UpdateAsync(LuceneAnalyzerSettings section, BuildEditorContext context)
        {
            if (context.GroupId == "search")
            {
                var model = new LuceneSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);
                if (!string.IsNullOrEmpty(model.Analyzer)) section.Analyzer = model.Analyzer;
                section.Version = model.Version;               
            }

            return await EditAsync(section, context);
        }
    }
}



