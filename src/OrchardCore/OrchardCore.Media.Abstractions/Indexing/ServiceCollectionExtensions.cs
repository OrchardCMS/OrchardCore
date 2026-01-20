using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Media.Indexing
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a new <see cref="IMediaFileTextProvider"/> to the <see cref="IServiceCollection"/> to process Media
        /// files with the given file extension.
        /// </summary>
        /// <typeparam name="TMediaFileTextProvider">
        /// The <see cref="IMediaFileTextProvider"/> implementation to process Media files with the given file
        /// extension.
        /// </typeparam>
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> to add the <see cref="IMediaFileTextProvider"/> implementation to.
        /// </param>
        /// <param name="fileExtension">
        /// The file extension of those files, wit a leading dot, that the given <see
        /// cref="IMediaFileTextProvider"/> implementation will process.
        /// </param>
        public static IServiceCollection AddMediaFileTextProvider<TMediaFileTextProvider>(this IServiceCollection services, string fileExtension)
            where TMediaFileTextProvider : class, IMediaFileTextProvider
        {
            return services.Configure<MediaFileIndexingOptions>(options => options.RegisterMediaFileTextProvider<TMediaFileTextProvider>(fileExtension));
        }
    }
}
