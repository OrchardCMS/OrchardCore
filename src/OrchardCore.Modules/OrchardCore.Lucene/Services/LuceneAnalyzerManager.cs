using System;
using System.Collections.Generic;
using Lucene.Net.Analysis;
using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace OrchardCore.Lucene.Services
{
   
    public class LuceneAnalyzerManager
    {
        private readonly ISiteService _siteService;
        ILuceneAnalyzerProviderManager _luceneAnalyzerProviderManager;
        public LuceneAnalyzerManager(
            ISiteService siteService,
            ILuceneAnalyzerProviderManager luceneAnalyzerProviderManager)
        {
            _siteService = siteService;
            _luceneAnalyzerProviderManager = luceneAnalyzerProviderManager;
        }

        public ILuceneAnalyzerProvider GetLuceneAnalyzerProvider()
        {
            var siteSettings = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();
            var culture = siteSettings.Culture;
            return _luceneAnalyzerProviderManager.GetLuceneAnalyzerProvider(culture);
        }
    }
}
