using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid;

public class HtmlContentValue : FluidValue
{
    private readonly IHtmlContent _value;

    public HtmlContentValue(IHtmlContent value)
    {
        _value = value;
    }

    public override FluidValues Type
        => FluidValues.String;

    public override bool Equals(FluidValue other)
    {
        if (other.IsNil())
        {
            return _value == null;
        }

        return _value == other;
    }

    public override ValueTask<FluidValue> GetValueAsync(string name, TemplateContext context)
    {
        return NilValue.Instance;
    }

    public override bool ToBooleanValue(TemplateContext context)
    {
        return true;
    }

    public override decimal ToNumberValue(TemplateContext context)
    {
        return 0;
    }

    public override string ToStringValue(TemplateContext context)
    {
        return _value.ToString();
    }

    public override ValueTask WriteToAsync(IFluidOutput output, TextEncoder encoder, TemplateContext context)
    {
        output.Write(_value, (HtmlEncoder)encoder);

        return ValueTask.CompletedTask;
    }

    public override object ToObjectValue(TemplateContext context)
    {
        return _value;
    }

    public override bool Equals(object other)
    {
        // The is operator will return false if null
        if (other is HtmlContentValue otherValue)
        {
            return _value.Equals(otherValue._value);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }
}
