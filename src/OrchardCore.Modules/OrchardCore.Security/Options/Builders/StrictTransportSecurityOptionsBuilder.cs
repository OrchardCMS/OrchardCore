using System;

namespace OrchardCore.Security.Options
{
    public class StrictTransportSecurityOptionsBuilder
    {
        private readonly StrictTransportSecurityOptions _options;

        public StrictTransportSecurityOptionsBuilder(StrictTransportSecurityOptions options)
            => _options = options ?? throw new ArgumentNullException(nameof(options));

        public StrictTransportSecurityOptionsBuilder WithMaxAge(TimeSpan maxAge)
        {
            _options.MaxAge = maxAge;

            return this;
        }

        public StrictTransportSecurityOptionsBuilder IncludeSubDomains(bool includeSubDomains = true)
        {
            _options.IncludeSubDomains = includeSubDomains;

            return this;
        }

        public StrictTransportSecurityOptionsBuilder Preload(bool preload = false)
        {
            _options.Preload = preload;

            return this;
        }
    }
}
