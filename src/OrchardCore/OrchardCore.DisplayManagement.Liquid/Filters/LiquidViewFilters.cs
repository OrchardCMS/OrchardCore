using System.Runtime.CompilerServices;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Localization;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Liquid.Filters;

public static class LiquidViewFilters
{
    [SkipLocalizationExtraction]
    public static ValueTask<FluidValue> Localize(FluidValue input, FilterArguments arguments, TemplateContext ctx)
    {
        var localizer = ctx.GetValue("ViewLocalizer")?.ToObjectValue() as IViewLocalizer;

        if (localizer == null)
        {
            return ThrowArgumentException<ValueTask<FluidValue>>("ViewLocalizer missing while invoking 't'");
        }

        // The 'note' named argument is a note for translators, read by the localization extraction, and is not a format
        // argument. The other named arguments are format arguments, whose names describe their placeholders.
        var note = arguments.HasNamed("note") ? arguments["note"] : null;
        var noteIndex = -1;
        if (note is not null)
        {
            for (var i = arguments.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(arguments.At(i), note))
                {
                    noteIndex = i;
                    break;
                }
            }
        }

        var count = noteIndex >= 0 ? arguments.Count - 1 : arguments.Count;
        var parameters = count > 0 ? new object[count] : [];
        for (int i = 0, j = 0; i < arguments.Count; i++)
        {
            if (i != noteIndex)
            {
                parameters[j++] = arguments.At(i).ToStringValue();
            }
        }

        return ValueTask.FromResult<FluidValue>(new StringValue(localizer.GetString(input.ToStringValue(), parameters)));
    }

    public static ValueTask<FluidValue> HtmlClass(FluidValue input, FilterArguments _1, TemplateContext _2)
    {
        return ValueTask.FromResult<FluidValue>(new StringValue(input.ToStringValue().HtmlClassify()));
    }

    public static ValueTask<FluidValue> ShapeProperties(FluidValue input, FilterArguments arguments, TemplateContext ctx)
    {
        if (input.ToObjectValue() is IShape shape)
        {
            foreach (var name in arguments.Names)
            {
                shape.Properties[name.ToPascalCaseUnderscore()] = arguments[name].ToObjectValue();
            }
            return FluidValue.Create(shape, ctx.Options);
        }

        return NilValue.Instance;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T ThrowArgumentException<T>(string message)
    {
        throw new ArgumentException(message);
    }
}
