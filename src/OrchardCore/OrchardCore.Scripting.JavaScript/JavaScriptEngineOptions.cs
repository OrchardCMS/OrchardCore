namespace OrchardCore.Scripting.JavaScript;

/// <summary>
/// Options controlling how <see cref="JavaScriptEngine"/> reuses Jint engines between evaluations.
/// </summary>
public class JavaScriptEngineOptions
{
    /// <summary>
    /// The number of idle engines a tenant keeps for reuse when nothing is configured.
    /// </summary>
    public const int DefaultEnginePoolSize = 8;

    /// <summary>
    /// Gets or sets how many idle engines the tenant keeps for reuse. Defaults to
    /// <see cref="DefaultEnginePoolSize"/>. Set it to <c>0</c> to build a new engine for every evaluation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is not a concurrency limit. An evaluation that starts while every pooled engine is in use builds
    /// its own, and releases it to the garbage collector afterwards, so raising or lowering the value can
    /// never make an evaluation wait or fail.
    /// </para>
    /// <para>
    /// What it bounds is retained state. A reused engine remembers the last object each script member access
    /// and each call resolved against, so a pooled engine can hold one such object — a request's
    /// <c>HttpContext</c>, a workflow execution context — per site in the scripts it has run, until that
    /// site next resolves something else. The default keeps that bounded to a handful of engines per tenant
    /// while comfortably covering the number of evaluations a site normally has in flight at once, since
    /// scripts are short and usually evaluated synchronously. Raise it for a tenant that evaluates scripts
    /// on many concurrent requests, and lower it — or set it to <c>0</c> — for one whose scripts project
    /// large object graphs into script and would rather not have them outlive the request.
    /// </para>
    /// </remarks>
    public int EnginePoolSize { get; set; } = DefaultEnginePoolSize;
}
