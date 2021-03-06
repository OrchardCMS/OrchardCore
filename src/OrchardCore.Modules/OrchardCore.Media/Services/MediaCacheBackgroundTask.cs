using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;

namespace OrchardCore.Media.Services
{

    // At 12:00 on Monday.
    [BackgroundTask(Schedule = "0 12 * * 1", Description = "Performs cleanup operations for ms-cache and is-cache folders periodically.")]
    // Left for test driving.
    //[BackgroundTask(Schedule = "* * * * *", Description = "Performs cleanup operations for ms-cache and is-cache folders periodically.")]
    public class MediaCacheBackgroundTask : IBackgroundTask
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger _logger;
        private const int Repeats = 5;
        private const int RepeatTime = 5000;

        public MediaCacheBackgroundTask(
        IWebHostEnvironment webHostEnvironment,
        ILogger<MediaCacheBackgroundTask> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Media cache background task cleaning started");

            var directoryInfo = new DirectoryInfo(Path.Combine(_webHostEnvironment.WebRootPath, "is-cache"));

            // Don't delete is-cache folder.
            await RecursiveDeleteAsync(directoryInfo, false, cancellationToken);

            // Prevents deletion of root tenant folders.
            directoryInfo = new DirectoryInfo(Path.Combine(_webHostEnvironment.WebRootPath, "ms-cache"));
            foreach (var dir in directoryInfo.EnumerateDirectories())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await RecursiveDeleteAsync(dir, false, cancellationToken);
            }

            return;
        }

        private async Task RecursiveDeleteAsync(DirectoryInfo baseDir, bool deleteBaseDir, CancellationToken cancellationToken)
        {
            if (!baseDir.Exists)
            {
                return;
            }

            try
            {
                var dirs = baseDir.EnumerateDirectories().ToArray();
                foreach (var dir in dirs)
                {
                    try
                    {
                        await RecursiveDeleteAsync(dir, true, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting dir {DirName}", dir.Name);
                    }
                }
            }
            catch (Exception ee)
            {
                _logger.LogError(ee, "Error enumerating dirs {DirName}", baseDir.Name);
            }

            var files = baseDir.GetFiles();
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    file.IsReadOnly = false;

                    if (IsFileLocked(file))
                    {
                        var i = 0;
                        while (file.Exists && IsFileLocked(file) && i <= Repeats)
                        {
                            await Task.Delay(RepeatTime, cancellationToken);
                            file.Delete();
                            i++;
                        }
                    }
                    else
                    {
                        file.Delete();
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error deleting cache file {FilePath}", file.Name);
                }
            }

            if (deleteBaseDir)
            {
                try
                {
                    var i = 0;
                    while (baseDir.Exists && baseDir.GetFiles().Length == 0 && i <= Repeats)
                    {
                        baseDir.Delete();
                        await Task.Delay(RepeatTime, cancellationToken);
                        i++;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error deleting cache folder {DirectoryPath}", baseDir.Name);
                }
            }
        }

        private static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                // The file is unavailable because it is:
                // still being written to
                // or being processed by another thread
                // or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            //file is not locked
            return false;
        }
    }
}
