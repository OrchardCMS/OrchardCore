// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Extensions.Configuration.Json;

/// <summary>
/// Represents a JSON file as an <see cref="IConfigurationSource"/>.
/// </summary>
public class TenantJsonStreamConfigurationSource : StreamConfigurationSource
{
    /// <summary>
    /// Builds the <see cref="TenantJsonStreamConfigurationProvider"/> for this source.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>An <see cref="TenantJsonStreamConfigurationProvider"/>.</returns>
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
        => new TenantJsonStreamConfigurationProvider(this);
}
