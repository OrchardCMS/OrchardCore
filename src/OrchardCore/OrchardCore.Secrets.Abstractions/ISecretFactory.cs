using System;

namespace OrchardCore.Secrets;

public interface ISecretFactory
{
    Type Type { get; }
    Secret Create();
}
