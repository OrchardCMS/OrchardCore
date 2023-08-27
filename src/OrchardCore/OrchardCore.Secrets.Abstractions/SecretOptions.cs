using System;
using System.Collections.Generic;

namespace OrchardCore.Secrets;

/// <summary>
/// Configuration options for <see cref="ISecretService"/>.
/// </summary>
public class SecretOptions
{
    /// <summary>
    /// The list of <see cref="Secret"/> types.
    /// </summary>
    public IList<Type> SecretTypes { get; set; } = new List<Type>();
}
