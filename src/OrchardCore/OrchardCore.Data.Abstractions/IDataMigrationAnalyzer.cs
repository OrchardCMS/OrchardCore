using System.Threading.Tasks;

namespace OrchardCore.Data.Migration
{
    /// <summary>
    /// Analyzes database migrations.
    /// </summary>
    public interface IDataMigrationAnalyzer
    {
        /// <summary>
        /// Analyzes data migrations related to the provided tenant name.
        /// </summary>
        Task<DataMigrationAnalyzerResult> AnalyzeAsync(string tenantName);
    }
}
