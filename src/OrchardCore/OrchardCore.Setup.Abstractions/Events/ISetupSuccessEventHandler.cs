using System.Threading.Tasks;

namespace OrchardCore.Setup.Events
{
    /// <summary>
    /// Setup success event handler.
    /// </summary>
    public interface ISetupSuccessEventHandler
    {
        /// <summary>
        /// This method get called after a successful tenant setup.
        /// </summary>
        Task SuccessAsync();
    }
}
