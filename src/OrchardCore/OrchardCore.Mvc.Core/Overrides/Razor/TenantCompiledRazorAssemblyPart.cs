// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.AspNetCore.Razor.Hosting;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts;

/// <summary>
/// An <see cref="ApplicationPart"/> for compiled Razor assemblies.
/// </summary>
public class TenantCompiledRazorAssemblyPart : ApplicationPart, IRazorCompiledItemProvider
{
    /// <summary>
    /// Initializes a new instance of <see cref="TenantCompiledRazorAssemblyPart"/>.
    /// </summary>
    /// <param name="assembly">The <see cref="System.Reflection.Assembly"/>The assembly.</param>
    public TenantCompiledRazorAssemblyPart(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        Assembly = assembly;
    }

    /// <summary>
    /// Gets the <see cref="System.Reflection.Assembly"/>.
    /// </summary>
    public Assembly Assembly { get; }

    /// <inheritdoc />
    public override string Name => Assembly.GetName().Name!;

    IEnumerable<RazorCompiledItem> IRazorCompiledItemProvider.CompiledItems
    {
        get
        {
            var loader = new TenantRazorCompiledItemLoader();
            return loader.LoadItems(Assembly);
        }
    }
}
