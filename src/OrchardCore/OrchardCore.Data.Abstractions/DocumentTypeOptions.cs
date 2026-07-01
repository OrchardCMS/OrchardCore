namespace OrchardCore.Data;

/// <summary>
/// Holds the CLR types that must be pre-registered with the store when a tenant is created.
/// </summary>
/// <remarks>
/// YesSql resolves the CLR type of a stored row from its persisted type name through a reverse
/// lookup that is only populated lazily, the first time a given type is saved or queried by its
/// exact type within a process. A by-id or polymorphic read (e.g. <c>GetAsync&lt;T&gt;(ids)</c>)
/// has no type filter, so it can load a row whose type has not been touched yet and fail with a
/// <see cref="KeyNotFoundException"/>. A module that stores such a type registers it here, for
/// example with <c>services.AddDocumentType&lt;TemplatesDocument&gt;()</c>, so the reverse lookup
/// always succeeds.
/// </remarks>
public class DocumentTypeOptions
{
    /// <summary>
    /// Gets the CLR types to pre-register with the store.
    /// </summary>
    public HashSet<Type> Types { get; } = [];
}
