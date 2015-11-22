using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Folders
{
    public interface IExtensionLocator
    {
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
    }
}