using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Documents;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using OrchardCore.Lists.Models;
using OrchardCore.Lucene;
using OrchardCore.Lucene.Model;
using OrchardCore.Lucene.Services;
using OrchardCore.Tests.Apis.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.Lucene
{
    public class RecentBlogPostsLuceneQueryTests
    {
        private Mock<LuceneIndexManager> _luceneIndexManager;
        private Mock<LuceneAnalyzerManager> _luceneAnalyzerManager;
        private Mock<ILuceneQueryService> _queryService;
        private Mock<ILiquidTemplateManager> _liquidTemplateManager;
        private Mock<LuceneIndexSettingsService> _luceneIndexSettingsService;
        private string IndexName = "Search";

        public RecentBlogPostsLuceneQueryTests()
        {
            _luceneIndexManager = new Mock<LuceneIndexManager>();
            _luceneAnalyzerManager = new Mock<LuceneAnalyzerManager>();
            _luceneIndexSettingsService = new Mock<LuceneIndexSettingsService>();
            _queryService = new Mock<ILuceneQueryService>();
            _liquidTemplateManager = new Mock<ILiquidTemplateManager>();
        }

        [Theory]
        [InlineData("{'from':0,'size':10,'query':{'bool':{'should':[{'wildcard':{'TitlePart':{'value':'test*','boost':5}}},{'wildcard':{'HtmlBodyPart':'test*'}}]}}}")]
        public async Task ShouldListBlogPostWhenCallingAQuery(string decodedQuery)
        {
            IEnumerable<Document> documents = Enumerable.Empty<Document>();
            int count = 0;

            using (var context = new BlogContext())
            {
                await context.InitializeAsync();

                var blogPostContentItemId = await context
                    .CreateContentItem("BlogPost", builder =>
                    {
                        builder.Published = true;
                        builder.Latest = true;
                        builder.DisplayText = "Some sorta blogpost in a Query!";

                        builder
                            .Weld(new ContainedPart
                            {
                                ListContentItemId = context.BlogContentItemId
                            });
                    });

                await _luceneIndexManager.Object.SearchAsync(IndexName, async searcher =>
                {
                    var analyzer = _luceneAnalyzerManager.Object.CreateAnalyzer(await _luceneIndexSettingsService.Object.GetIndexAnalyzerAsync(IndexName));
                    var context = new LuceneQueryContext(searcher, LuceneSettings.DefaultVersion, analyzer);

                    //var templateContext = _liquidTemplateManager.Object.Context;
                    //var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(model.Parameters);

                    //foreach (var parameter in parameters)
                    //{
                    //    templateContext.SetValue(parameter.Key, parameter.Value);
                    //}

                    //var tokenizedContent = await _liquidTemplateManager.RenderAsync(decodedQuery, _javaScriptEncoder);

                    try
                    {
                        //var parameterizedQuery = JObject.Parse(decodedQuery);
                        var luceneTopDocs = await _queryService.Object.SearchAsync(context, JObject.Parse(decodedQuery));

                        if (luceneTopDocs != null)
                        {
                            documents = luceneTopDocs.TopDocs.ScoreDocs.Select(hit => searcher.Doc(hit.Doc)).ToList();
                            count = luceneTopDocs.Count;
                        }
                    }
                    catch
                    {
                        //_logger.LogError(e, "Error while executing query");
                    }
                });
            }
        }
    }
}
