using Orchard.Autoroute.Model;
using System;
using System.Threading.Tasks;

namespace Orchard.Autoroute.Services
{
    public interface IAutorouteValidator
    {
        Task ValidateAsync(AutoroutePart autoroute, Action<string, string> reportError);
    }
}
