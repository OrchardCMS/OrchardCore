using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Setup.Events
{
    /// <summary>
    /// Contract that is called when a tenant is set up.
    /// </summary>
    [Obsolete($"This interface is obsolute and will be removed in future releases. Please use '{nameof(ITenantSetupHandler)}' instead.")]
    public interface ISetupEventHandler
    {
        Task Setup(IDictionary<string, object> properties, Action<string, string> reportError);
    }
}
