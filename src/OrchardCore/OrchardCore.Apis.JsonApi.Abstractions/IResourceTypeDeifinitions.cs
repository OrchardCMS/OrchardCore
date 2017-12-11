using System.Collections.Generic;

namespace OrchardCore.Apis.JsonApi
{
    public interface IResourceTypeDeifinitions
    {
        IEnumerable<string> Types { get; }
    }
}
