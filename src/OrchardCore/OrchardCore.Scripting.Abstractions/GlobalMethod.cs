using System;

namespace OrchardCore.Scripting
{
    public class GlobalMethod
    {
        public string Name { get; set; }
        public Func<IServiceProvider, Delegate> Method { get; set; }
    }
}
