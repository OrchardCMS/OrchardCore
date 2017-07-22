using System;
using System.Threading.Tasks;
using Orchard.Events;

namespace Orchard.Setup.Services
{
    /// <summary>
    /// Called when a tenant is set up.
    /// </summary>
    public interface ISetupEventHandler : IEventHandler
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
