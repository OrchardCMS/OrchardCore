using Fluid;
using Microsoft.Extensions.Options;

namespace OrchardCore.Liquid;

/// <summary>
/// Provides the <see cref="TemplateOptions"/> of a tenant, built once from the <see cref="TemplateOptionsBuilder"/>
/// that the modules configure. <see cref="TemplateOptions"/> is immutable and can't be created by the options system.
/// </summary>
internal sealed class TemplateOptionsAccessor : IOptions<TemplateOptions>
{
    private readonly Lazy<TemplateOptions> _value;

    public TemplateOptionsAccessor(IOptions<TemplateOptionsBuilder> builder, IEnumerable<IConfigureOptions<TemplateOptions>> ignoredSetups)
    {
        // Such a registration compiles but would never run, leaving its filters or members silently unavailable to the templates.
        foreach (var setup in ignoredSetups)
        {
            var source = (setup as ConfigureNamedOptions<TemplateOptions>)?.Action?.Method.DeclaringType ?? setup.GetType();

            throw new InvalidOperationException(
                $"Configuring '{nameof(TemplateOptions)}' isn't supported as it is immutable, configure '{nameof(TemplateOptionsBuilder)}' instead. Used by '{source.FullName}'.");
        }

        _value = new Lazy<TemplateOptions>(() => builder.Value.Build());
    }

    public TemplateOptions Value => _value.Value;
}
