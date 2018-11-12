using Lucene.Net.Analysis;

namespace OrchardCore.Lucene.Services
{
    public interface ILuceneAnalyzerProviderManager
    {        
        ILuceneAnalyzerProvider GetLuceneAnalyzerProvider(string key = "");
    }
}