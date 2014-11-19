using System;
using System.Linq;
using System.Reflection;

namespace OrchardVNext.Environment {
    public interface IAssemblyNameResolver {
        int Order { get; }

        /// <summary>
        /// Resolve a short assembly name to a full name
        /// </summary>
        string Resolve(string shortName);
    }

    public class OrchardFrameworkAssemblyNameResolver : IAssemblyNameResolver {

        public int Order { get { return 20; } }

        public string Resolve(string shortName) {
            Console.WriteLine("TODO: Add in Assembly Name Resolver");

            return null;
        }
    }
}
