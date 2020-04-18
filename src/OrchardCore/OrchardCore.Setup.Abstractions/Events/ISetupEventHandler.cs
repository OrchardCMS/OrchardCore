using System;
using System.Threading.Tasks;

namespace OrchardCore.Setup.Events
{
    /// <summary>
    /// Contract that is called when a tenant is set up.
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
            string siteTimeZone,
            bool useCdn,
            Action<string, string> reportError
            );
    }
}
