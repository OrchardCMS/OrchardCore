using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Media.Indexing
{
    public class MediaFileIndexingOptions
    {
        private readonly Dictionary<string, Type> _mediaFileTextProviderRegistrations = new Dictionary<string, Type>();

        public MediaFileIndexingOptions RegisterMediaFileTextProvider<TMediaFileTextProvider>(string fileExtension)
            where TMediaFileTextProvider : class, IMediaFileTextProvider
        {
            return RegisterMediaFileTextProvider(fileExtension, typeof(TMediaFileTextProvider));
        }

        public MediaFileIndexingOptions RegisterMediaFileTextProvider(string fileExtension, Type providerType)
        {
            // Deliberate overwrite behavior so you can override registrations.
            _mediaFileTextProviderRegistrations[NormalizeFileExtension(fileExtension)] = providerType;

            return this;
        }

        public Type GetRegisteredMediaFileTextProvider(string fileExtension)
        {
            if (_mediaFileTextProviderRegistrations.TryGetValue(NormalizeFileExtension(fileExtension), out var providerType))
            {
                return providerType;
            }

            return null;
        }

        public bool AnyMediaFileTextProviders()
        {
            return _mediaFileTextProviderRegistrations.Any();
        }

        private static string NormalizeFileExtension(string fileExtension)
        {
            if (fileExtension.StartsWith('.'))
            {
                throw new ArgumentException("The file extension should be just the extension and not start with a dot.");
            }

            return fileExtension.ToUpperInvariant();
        }
    }
}
