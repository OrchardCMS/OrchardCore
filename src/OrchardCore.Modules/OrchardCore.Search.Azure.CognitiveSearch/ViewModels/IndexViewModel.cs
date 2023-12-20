using System;

namespace OrchardCore.Search.Azure.CognitiveSearch.ViewModels;

public class IndexViewModel
{
    public string Name { get; set; }

    public string AnalyzerName { get; set; }

    public DateTime LastUpdateUtc { get; set; }
}
