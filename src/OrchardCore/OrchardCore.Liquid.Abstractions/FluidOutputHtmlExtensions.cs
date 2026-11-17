using System.Text.Encodings.Web;
using Fluid;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.Liquid;

/// <summary>
/// Writes to an <see cref="IFluidOutput"/> from the components that render to a <see cref="TextWriter"/>.
/// </summary>
public static class FluidOutputHtmlExtensions
{
    /// <summary>
    /// Gets a <see cref="TextWriter"/> writing to the output, which is the output itself when it is also a
    /// <see cref="TextWriter"/>. The writer doesn't need to be disposed.
    /// </summary>
    public static TextWriter AsTextWriter(this IFluidOutput output)
        => output as TextWriter ?? new FluidOutputTextWriter(output);

    /// <summary>
    /// Writes an <see cref="IHtmlContent"/> to the output.
    /// </summary>
    public static void Write(this IFluidOutput output, IHtmlContent content, HtmlEncoder encoder)
        => content.WriteTo(output.AsTextWriter(), encoder);
}
