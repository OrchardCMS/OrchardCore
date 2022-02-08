using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.BackgroundJobs.Models;

namespace OrchardCore.BackgroundJobs.Services
{
    public interface IBackgroundJobEntries
    {
        /// <summary>
        /// Lists all entries
        /// </summary>
        /// <returns></returns>
        ValueTask<IEnumerable<BackgroundJobEntry>> GetEntriesAsync();

        /// <summary>
        /// Removes a <see cref="BackgroundJobEntry"/> by <see cref="BackgroundJobEntry.BackgroundJobId"/>
        /// </summary>
        /// <param name="backgroundJobId"></param>
        ValueTask RemoveEntryAsync(string backgroundJobId);

        /// <summary>
        /// Adds or updates a <see cref="BackgroundJobEntry"/>
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        ValueTask AddOrUpdateEntryAsync(BackgroundJobEntry entry);
    }
}
