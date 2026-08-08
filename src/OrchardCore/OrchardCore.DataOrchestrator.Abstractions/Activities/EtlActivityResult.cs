namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Represents the result of executing an ETL activity.
/// </summary>
public sealed class EtlActivityResult
{
    private EtlActivityResult(IReadOnlyList<string> outcomes, bool isSuccess, string errorMessage)
    {
        Outcomes = outcomes;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Gets the outcome names produced by the activity execution.
    /// </summary>
    public IReadOnlyList<string> Outcomes { get; }

    /// <summary>
    /// Gets whether the activity executed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the error message if the activity failed.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Creates a successful result with the specified outcomes.
    /// </summary>
    public static EtlActivityResult Success(params string[] outcomes)
    {
        return new EtlActivityResult(outcomes, isSuccess: true, errorMessage: null);
    }

    /// <summary>
    /// Creates a failure result with the specified error message.
    /// </summary>
    public static EtlActivityResult Failure(string error)
    {
        return new EtlActivityResult([], isSuccess: false, errorMessage: error);
    }
}
