using System;
using System.Threading.Tasks;

namespace Orchard.Setup.Events
{
    /// <summary>
    /// Called when a tenant is set up.
    /// </summary>
    public interface ISetupEventHandler
    {
        Task Setup(
            string siteName,
            string userName,
            string email,
            string password,
            string dbProvider,
            string dbConnectionString,
            string dbTablePrefix,
            Action<string, string> reportError
            );
    }
}
