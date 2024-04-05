using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;

namespace OrchardCore.Media.Services
{
    public class SecureMediaDirectoryChangeHelper
    {
        private const string DirectoriesChangedIdKey = "SecureMedia_Directories_ChangedId";

        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<SecureMediaDirectoryChangeHelper> _logger;
        private string _currentChangeKey;

        public SecureMediaDirectoryChangeHelper(IDistributedCache cache, ILogger<SecureMediaDirectoryChangeHelper> logger)
        {
            _distributedCache = cache;
            _logger = logger;
        }

        public async Task<bool> DetectChangesAsync()
        {
            try
            {
                var changeKey = await _distributedCache.GetStringAsync(DirectoriesChangedIdKey);
                if (changeKey is null || changeKey == _currentChangeKey)
                {
                    return false;
                }

                _currentChangeKey = changeKey;
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Unable to read the distributed cache.");
            }

            // Force to rebuild the local permission cache in case we got an error from the distributed cache.
            return true;
        }

        public async Task UpdateAsync()
        {
            try
            {
                var changeKey = IdGenerator.GenerateId();
                await _distributedCache.SetStringAsync(DirectoriesChangedIdKey, changeKey);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Unable to update the distributed cache.");
            }
        }
    }
}
