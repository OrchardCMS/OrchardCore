using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Media.Services
{
    public class TempDirCleanerService : ModularTenantEvents
    {
        private readonly IMediaFileStore _fileStore;
        private readonly AttachedMediaFieldFileService _attachedMediaFieldFileService;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;

        public TempDirCleanerService(IMediaFileStore fileStore,
            AttachedMediaFieldFileService attachedMediaFieldFileService,
            ShellSettings shellSettings,
            ILogger<TempDirCleanerService> logger)
        {
            _fileStore = fileStore;
            _attachedMediaFieldFileService = attachedMediaFieldFileService;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        public override async Task ActivatedAsync()
        {
            if (_shellSettings.IsRunning())
            {
                try
                {
                    var tempDir = _attachedMediaFieldFileService.MediaFieldsTempSubFolder;

                    if (await _fileStore.GetDirectoryInfoAsync(tempDir) == null)
                    {
                        return;
                    }

                    await foreach (var c in _fileStore.GetDirectoryContentAsync(tempDir))
                    {
                        var result = c.IsDirectory ?
                            await _fileStore.TryDeleteDirectoryAsync(c.Path)
                            : await _fileStore.TryDeleteFileAsync(c.Path);

                        if (!result)
                        {
                            _logger.LogWarning("Temporary entry {Path} could not be deleted.", c.Path);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error occurred while cleaning temporary media folder.");
                }
            }
        }
    }
}
