// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts;

/// <summary>
/// An <see cref="ApplicationPart"/> backed by an <see cref="System.Reflection.Assembly"/>.
/// </summary>
public class TenantAssemblyPart : ApplicationPart, IApplicationPartTypeProvider
{
    /// <summary>
    /// Initializes a new <see cref="AssemblyPart"/> instance.
    /// </summary>
    /// <param name="assembly">The backing <see cref="System.Reflection.Assembly"/>.</param>
    public TenantAssemblyPart(Assembly assembly)
    {
        Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
    }

    /// <summary>
    /// Gets the <see cref="Assembly"/> of the <see cref="ApplicationPart"/>.
    /// </summary>
    public Assembly Assembly { get; }

    /// <summary>
    /// Gets the name of the <see cref="ApplicationPart"/>.
    /// </summary>
    public override string Name => Assembly.GetName().Name!;

    /// <inheritdoc />
    public IEnumerable<TypeInfo> Types
    {
        get
        {
            var types = Assembly.DefinedTypes
                .Where(t => t.Name != nameof(CompiledRazorAssemblyPart))
                .ToList();

            types.Add(typeof(TenantCompiledRazorAssemblyPart).GetTypeInfo());

            return types;
        }
    }
}
