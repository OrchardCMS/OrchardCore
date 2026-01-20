using System;
using System.Collections.Generic;

namespace OrchardCore.Media.Indexing
{
    public class MediaFileIndexingOptions
    {
        private readonly Dictionary<string, Type> _mediaFileTextProviderRegistrations = new(StringComparer.OrdinalIgnoreCase);

        public MediaFileIndexingOptions RegisterMediaFileTextProvider<TMediaFileTextProvider>(string fileExtension)
            where TMediaFileTextProvider : class, IMediaFileTextProvider
        {
            return RegisterMediaFileTextProvider(fileExtension, typeof(TMediaFileTextProvider));
        }

        public MediaFileIndexingOptions RegisterMediaFileTextProvider(string fileExtension, Type providerType)
        {
            // Deliberate overwrite behavior so you can override registrations.
            _mediaFileTextProviderRegistrations[ValidateFileExtension(fileExtension)] = providerType;

            return this;
        }

        public Type GetRegisteredMediaFileTextProvider(string fileExtension)
        {
            if (_mediaFileTextProviderRegistrations.TryGetValue(ValidateFileExtension(fileExtension), out var providerType))
            {
                return providerType;
            }

            return null;
        }

        private static string ValidateFileExtension(string fileExtension)
        {
            if (!fileExtension.StartsWith('.'))
            {
                throw new ArgumentException("The file extension should start with a dot.");
            }

            return fileExtension;
        }
    }
}
