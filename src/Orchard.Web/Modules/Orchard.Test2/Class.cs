using Orchard.DependencyInjection;

namespace Orchard.Test2 {
    public interface IFoo : IDependency {}
    public class Class : IFoo {
    }
}
