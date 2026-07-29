using System.Runtime.CompilerServices;

namespace OrchardCore.Tests.Scripting;

/// <summary>
/// Turns on Jint's host-contract verifiers for the whole test run.
/// </summary>
/// <remarks>
/// <para>
/// The verifiers catch an object projected into script answering one of Jint's extension points in a way
/// that contradicts another — a key that exists but is invisible to <c>Object.keys</c>, a read that resolves
/// on the prototype for a property the object actually owns. The engine trusts those hooks on its fast paths
/// rather than re-checking them, so a violation is otherwise silent rather than loud.
/// </para>
/// <para>
/// Orchard Core defines no Jint types of its own today, so this currently has little to check. It is here as
/// a tripwire: the moment a module projects a custom object into script — or a change to the wrap handler
/// starts returning one — the contract is verified by the existing test run instead of by whatever
/// misbehaves in production.
/// </para>
/// <para>
/// The switch has to be set before the first use of any Jint type, because Jint reads it once at type
/// initialization, hence the module initializer. It is deliberately confined to this test assembly: the
/// verifiers redo work the paths they check exist to avoid, so this must not be set in a hosting app.
/// </para>
/// </remarks>
internal static class JintHostContractVerification
{
    [ModuleInitializer]
    internal static void Enable()
        => AppContext.SetSwitch("Jint.EnableHostContractVerification", true);
}
