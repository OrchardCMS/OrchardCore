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
    /// <c>HttpContext</c>, a workflow execution context, or the services of the request a registered global
    /// was built for — per site in the scripts it has run. The pool discards that state once it is a minute
    /// old, when the engine is next returned or, if it is idle by then, by a background sweep, so a finished
    /// request's objects stay reachable for at most about two minutes; the cost is that each script runs once
    /// more as if on a new engine, minus building the engine. The default keeps the engines that can hold
    /// such state to a handful per tenant while comfortably covering the number of evaluations a site
    /// normally has in flight at once, since scripts are short and usually evaluated synchronously. Raise it
    /// for a tenant that evaluates scripts on many concurrent requests, and set it to <c>0</c> for one whose
    /// scripts project large object graphs into script and must not have them outlive the request at all.
    /// </para>
    /// </remarks>
    public int EnginePoolSize { get; set; } = DefaultEnginePoolSize;

    /// <summary>
    /// The clock the pool reads to decide when an engine's interpreter state is old enough to discard, and
    /// schedules its sweep on. Settable so that tests can move time forward.
    /// </summary>
    internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;
}
