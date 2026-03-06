using Microsoft.Extensions.Localization;

namespace OrchardCore.Infrastructure;

/// <summary>
/// Represents the result of an operation.
/// </summary>
public class Result
{
    private static readonly Result _success = new Result { Succeeded = true };
    private readonly List<ResultError> _errors = [];

    private Result()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class with the specified success status and errors.
    /// </summary>
    /// <param name="succeeded">Indicates whether the operation succeeded.</param>
    /// <param name="errors">The errors that occurred during the operation.</param>
    protected Result(bool succeeded, List<ResultError> errors)
    {
        Succeeded = succeeded;
        _errors = errors;
    }

    /// <summary>
    /// Gets the collection of errors associated with the result.
    /// </summary>
    public IEnumerable<ResultError> Errors => _errors;

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool Succeeded { get; protected set; }

    /// <summary>
    /// Returns a successful result instance.
    /// </summary>
    /// <returns>A successful result instance.</returns>
    public static Result Success() => _success;

    /// <summary>
    /// Returns a successful result instance with the specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value returned by the operation.</typeparam>
    /// <param name="value">The value returned by the operation.</param>
    /// <returns>A successful result instance with the specified value.</returns>
    public static Result<TValue> Success<TValue>(TValue value)
        => new Result<TValue>(value, true, []);

    /// <summary>
    /// Returns a failed result instance with the specified errors.
    /// </summary>
    /// <param name="errors">The errors that occurred during the operation.</param>
    /// <returns>A failed result instance with the specified errors.</returns>
    public static Result Failed(params IEnumerable<ResultError> errors)
    {
        var result = new Result();

        if (errors is not null)
        {
            result._errors.AddRange(errors);
        }

        return result;
    }

    /// <summary>
    /// Returns a failed result instance with the specified errors.
    /// </summary>
    /// <param name="errors">The errors that occurred during the operation.</param>
    /// <returns>A failed result instance with the specified errors.</returns>
    public static Result Failed(params ResultError[] errors) => Failed(errors.AsEnumerable());

    /// <summary>
    /// Returns a failed result instance with the specified error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed result instance with the specified error message.</returns>
    public static Result Failed(LocalizedString error) => Failed(new ResultError
    {
        Message = error,
    });

    /// <summary>
    /// Returns a failed result instance with the specified error message.
    /// </summary>
    /// <typeparam name="TValue">The type of the value returned by the operation.</typeparam>
    /// <param name="errors">The errors that occurred during the operation.</param>
    /// <returns>A failed result instance with the specified error message.</returns>
    public static Result<TValue> Failed<TValue>(params ResultError[] errors)
        => new Result<TValue>(default, false, errors.ToList());
}
