using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;

namespace OrchardVNext.Test2 {
    public interface IFoo : IDependency {}
    public class Class : IFoo {
        public Class() { }
    }
}
