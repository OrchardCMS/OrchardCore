using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Tests.Stubs
{
    public class StubExtensionExpanderOptions : IOptions<ExtensionExpanderOptions>
    {
        private ExtensionExpanderOption[] _options;
        public StubExtensionExpanderOptions(params ExtensionExpanderOption[] options)
        {
            _options = options;
        }

        public ExtensionExpanderOptions Value
        {
            get
            {
                ExtensionExpanderOptions options = new ExtensionExpanderOptions();

                foreach (var option in _options)
                {
                    options.Options.Add(option);
                }

                return options;
            }
        }
    }
}