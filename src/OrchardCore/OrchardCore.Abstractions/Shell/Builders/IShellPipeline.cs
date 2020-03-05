using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Builders
{
    public interface IShellPipeline
    {
        /// <summary>
        /// Executes this shell pipeline.
        /// </summary>
        Task Invoke(object context);
    }
}
