using Azure;
using Azure.Search.Documents.Indexes.Models;

namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchDefaultOptions
{
    public const string DefaultAnalyzer = LexicalAnalyzerName.Values.StandardLucene;

    public static readonly string[] DefaultAnalyzers =
    [
        LexicalAnalyzerName.Values.ArMicrosoft,
        LexicalAnalyzerName.Values.ArLucene,
        LexicalAnalyzerName.Values.HyLucene,
        LexicalAnalyzerName.Values.BnMicrosoft,
        LexicalAnalyzerName.Values.EuLucene,
        LexicalAnalyzerName.Values.BgMicrosoft,
        LexicalAnalyzerName.Values.BgLucene,
        LexicalAnalyzerName.Values.CaMicrosoft,
        LexicalAnalyzerName.Values.CaLucene,
        LexicalAnalyzerName.Values.ZhHansMicrosoft,
        LexicalAnalyzerName.Values.ZhHansLucene,
        LexicalAnalyzerName.Values.ZhHantMicrosoft,
        LexicalAnalyzerName.Values.ZhHantLucene,
        LexicalAnalyzerName.Values.HrMicrosoft,
        LexicalAnalyzerName.Values.CsMicrosoft,
        LexicalAnalyzerName.Values.CsLucene,
        LexicalAnalyzerName.Values.DaMicrosoft,
        LexicalAnalyzerName.Values.DaLucene,
        LexicalAnalyzerName.Values.NlMicrosoft,
        LexicalAnalyzerName.Values.NlLucene,
        LexicalAnalyzerName.Values.EnMicrosoft,
        LexicalAnalyzerName.Values.EnLucene,
        LexicalAnalyzerName.Values.EtMicrosoft,
        LexicalAnalyzerName.Values.FiMicrosoft,
        LexicalAnalyzerName.Values.FiLucene,
        LexicalAnalyzerName.Values.FrMicrosoft,
        LexicalAnalyzerName.Values.FrLucene,
        LexicalAnalyzerName.Values.GlLucene,
        LexicalAnalyzerName.Values.DeMicrosoft,
        LexicalAnalyzerName.Values.DeLucene,
        LexicalAnalyzerName.Values.ElMicrosoft,
        LexicalAnalyzerName.Values.ElLucene,
        LexicalAnalyzerName.Values.GuMicrosoft,
        LexicalAnalyzerName.Values.HeMicrosoft,
        LexicalAnalyzerName.Values.HiMicrosoft,
        LexicalAnalyzerName.Values.HiLucene,
        LexicalAnalyzerName.Values.HuMicrosoft,
        LexicalAnalyzerName.Values.HuLucene,
        LexicalAnalyzerName.Values.IsMicrosoft,
        LexicalAnalyzerName.Values.IdMicrosoft,
        LexicalAnalyzerName.Values.GaLucene,
        LexicalAnalyzerName.Values.ItMicrosoft,
        LexicalAnalyzerName.Values.ItLucene,
        LexicalAnalyzerName.Values.JaMicrosoft,
        LexicalAnalyzerName.Values.JaLucene,
        LexicalAnalyzerName.Values.KnMicrosoft,
        LexicalAnalyzerName.Values.KoMicrosoft,
        LexicalAnalyzerName.Values.KoLucene,
        LexicalAnalyzerName.Values.LvMicrosoft,
        LexicalAnalyzerName.Values.LvLucene,
        LexicalAnalyzerName.Values.LtMicrosoft,
        LexicalAnalyzerName.Values.MlMicrosoft,
        LexicalAnalyzerName.Values.MsMicrosoft,
        LexicalAnalyzerName.Values.MrMicrosoft,
        LexicalAnalyzerName.Values.NbMicrosoft,
        LexicalAnalyzerName.Values.NoLucene,
        LexicalAnalyzerName.Values.FaLucene,
        LexicalAnalyzerName.Values.PlMicrosoft,
        LexicalAnalyzerName.Values.PlLucene,
        LexicalAnalyzerName.Values.PtBrMicrosoft,
        LexicalAnalyzerName.Values.PtBrLucene,
        LexicalAnalyzerName.Values.PtPtMicrosoft,
        LexicalAnalyzerName.Values.PtPtLucene,
        LexicalAnalyzerName.Values.PaMicrosoft,
        LexicalAnalyzerName.Values.RoMicrosoft,
        LexicalAnalyzerName.Values.RoLucene,
        LexicalAnalyzerName.Values.RuMicrosoft,
        LexicalAnalyzerName.Values.RuLucene,
        LexicalAnalyzerName.Values.SrCyrillicMicrosoft,
        LexicalAnalyzerName.Values.SrLatinMicrosoft,
        LexicalAnalyzerName.Values.SkMicrosoft,
        LexicalAnalyzerName.Values.SlMicrosoft,
        LexicalAnalyzerName.Values.EsMicrosoft,
        LexicalAnalyzerName.Values.EsLucene,
        LexicalAnalyzerName.Values.SvMicrosoft,
        LexicalAnalyzerName.Values.SvLucene,
        LexicalAnalyzerName.Values.TaMicrosoft,
        LexicalAnalyzerName.Values.TeMicrosoft,
        LexicalAnalyzerName.Values.ThMicrosoft,
        LexicalAnalyzerName.Values.ThLucene,
        LexicalAnalyzerName.Values.TrMicrosoft,
        LexicalAnalyzerName.Values.TrLucene,
        LexicalAnalyzerName.Values.UkMicrosoft,
        LexicalAnalyzerName.Values.UrMicrosoft,
        LexicalAnalyzerName.Values.ViMicrosoft,
        LexicalAnalyzerName.Values.StandardLucene,
        LexicalAnalyzerName.Values.StandardAsciiFoldingLucene,
        LexicalAnalyzerName.Values.Keyword,
        LexicalAnalyzerName.Values.Pattern,
        LexicalAnalyzerName.Values.Simple,
        LexicalAnalyzerName.Values.Stop,
        LexicalAnalyzerName.Values.Whitespace,
    ];

    public string Endpoint { get; set; }

    public AzureAIAuthenticationType AuthenticationType { get; set; }

    public bool DisableUIConfiguration { get; set; }

    public AzureKeyCredential Credential { get; set; }

    // Environment prefix for all of the indexes.
    public string IndexesPrefix { get; set; }

    public string[] Analyzers { get; set; }

    public string IdentityClientId { get; set; }

    private bool _configurationExists;

    public void SetConfigurationExists(bool configurationExists)
        => _configurationExists = configurationExists;

    public bool ConfigurationExists()
        => _configurationExists;

    private bool _fileConfigurationExists;

    public void SetFileConfigurationExists(bool fileConfigurationExists)
        => _fileConfigurationExists = fileConfigurationExists;

    public bool FileConfigurationExists()
        => _fileConfigurationExists;
}
