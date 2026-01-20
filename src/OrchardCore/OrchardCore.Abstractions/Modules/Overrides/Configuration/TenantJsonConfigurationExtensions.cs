// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;

#nullable enable

namespace Microsoft.Extensions.Configuration;

/// <summary>
/// Extension methods for adding <see cref="JsonConfigurationProvider"/>.
/// </summary>
public static class TenantJsonConfigurationExtensions
{
    /// <summary>
    /// Adds the JSON configuration provider at <paramref name="path"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="path">Path relative to the base path stored in
    /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddTenantJsonFile(this IConfigurationBuilder builder, string path)
    {
        return AddTenantJsonFile(builder, provider: null, path: path, optional: false, reloadOnChange: false);
    }

    /// <summary>
    /// Adds the JSON configuration provider at <paramref name="path"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="path">Path relative to the base path stored in
    /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
    /// <param name="optional">Whether the file is optional.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddTenantJsonFile(this IConfigurationBuilder builder, string path, bool optional)
    {
        return AddTenantJsonFile(builder, provider: null, path: path, optional: optional, reloadOnChange: false);
    }

    /// <summary>
    /// Adds the JSON configuration provider at <paramref name="path"/> to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="path">Path relative to the base path stored in
    /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
    /// <param name="optional">Whether the file is optional.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddTenantJsonFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
    {
        return AddTenantJsonFile(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);
    }

    /// <summary>
    /// Adds a JSON configuration source to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="provider">The <see cref="IFileProvider"/> to use to access the file.</param>
    /// <param name="path">Path relative to the base path stored in
    /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
    /// <param name="optional">Whether the file is optional.</param>
    /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddTenantJsonFile(this IConfigurationBuilder builder, IFileProvider? provider, string path, bool optional, bool reloadOnChange)
    {
        // ThrowHelper.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(builder);

        if (string.IsNullOrEmpty(path))
        {
            // throw new ArgumentException(SR.Error_InvalidFilePath, nameof(path));
            throw new ArgumentException("File path must be a non-empty string.", nameof(path));
        }

        return builder.AddTenantJsonFile(s =>
        {
            s.FileProvider = provider;
            s.Path = path;
            s.Optional = optional;
            s.ReloadOnChange = reloadOnChange;
            s.ResolveFileProvider();
        });
    }

    /// <summary>
    /// Adds a JSON configuration source to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="configureSource">Configures the source.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddTenantJsonFile(this IConfigurationBuilder builder, Action<TenantJsonConfigurationSource>? configureSource)
        => builder.Add(configureSource);

    /// <summary>
    /// Adds a JSON configuration source to <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="stream">The <see cref="Stream"/> to read the json configuration data from.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddTenantJsonStream(this IConfigurationBuilder builder, Stream stream)
    {
        // ThrowHelper.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(builder);

        return builder.Add<TenantJsonStreamConfigurationSource>(s => s.Stream = stream);
    }
}
