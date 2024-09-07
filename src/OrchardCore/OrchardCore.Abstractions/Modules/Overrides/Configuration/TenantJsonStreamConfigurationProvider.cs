// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using OrchardCore.Environment.Shell.Configuration.Internal;

namespace Microsoft.Extensions.Configuration.Json;

/// <summary>
/// Loads configuration key/values from a json stream into a provider.
/// </summary>
public class TenantJsonStreamConfigurationProvider : StreamConfigurationProvider
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="source">The <see cref="TenantJsonStreamConfigurationSource"/>.</param>
    public TenantJsonStreamConfigurationProvider(TenantJsonStreamConfigurationSource source) : base(source) { }

    /// <summary>
    /// Loads json configuration key/values from a stream into a provider.
    /// </summary>
    /// <param name="stream">The json <see cref="Stream"/> to load configuration data from.</param>
    public override void Load(Stream stream)
    {
        Data = JsonConfigurationParser.Parse(stream);
    }
}
