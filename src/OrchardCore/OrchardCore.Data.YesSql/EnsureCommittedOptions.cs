namespace OrchardCore.Data;

/// <summary>
/// Options for controlling when and how database commits are performed during HTTP requests.
/// </summary>
public class EnsureCommittedOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether automatic commit-before-response is enabled.
    /// Default is true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the behavior when a commit fails.
    /// Default is ThrowOnCommitFailure to prevent returning success when writes didn't persist.
    /// </summary>
    public CommitFailureBehavior FailureBehavior { get; set; } = CommitFailureBehavior.ThrowOnCommitFailure;

    /// <summary>
    /// Gets or sets an optional list of paths that should trigger commits.
    /// When non-empty, only requests matching these paths will be committed.
    /// Default is empty (all requests are committed).
    /// </summary>
    public string[] FlushOnPaths { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Defines how the system should behave when a database commit fails.
/// </summary>
public enum CommitFailureBehavior
{
    /// <summary>
    /// Throw an exception when commit fails, preventing the response from being sent.
    /// </summary>
    ThrowOnCommitFailure,

    /// <summary>
    /// Log the exception but allow the response to be sent.
    /// </summary>
    LogOnly,
}
