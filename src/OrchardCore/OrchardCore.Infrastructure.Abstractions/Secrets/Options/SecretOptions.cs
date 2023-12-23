using System;
using System.Collections.Generic;

namespace OrchardCore.Secrets.Options;

/// <summary>
/// Configuration options for <see cref="ISecretService"/>.
/// </summary>
public class SecretOptions
{
    /// <summary>
    /// The list of secret types.
    /// </summary>
    public IList<Type> Types { get; set; } = new List<Type>();

    /// <summary>
    /// The list of secret purposes.
    /// </summary>
    public IList<string> Purposes { get; set; } = new List<string>();
}
