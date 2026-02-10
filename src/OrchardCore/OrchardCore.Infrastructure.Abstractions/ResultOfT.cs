using Microsoft.Extensions.Localization;

namespace OrchardCore.Infrastructure;

public class Result<TValue> : Result
{
    public TValue Value { get; set; }

    protected internal Result(TValue value, bool succeeded, IEnumerable<LocalizedString> errors) : base(succeeded, errors)
    {
        Value = value;
    }
}
