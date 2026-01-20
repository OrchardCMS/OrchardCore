using System.Threading.Tasks;

namespace OrchardCore.Seo;

public interface IRobotsProvider
{
    /// <summary>
    /// Provides a way to contribute to the content of the robots.txt file.
    /// </summary>
    /// <returns>Content to add to the robots.txt file.</returns>
    Task<string> GetContentAsync();
}
