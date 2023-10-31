using System;

namespace OrchardCore.ContentsTransfer.Models;

public class ContentImportOptions
{
    public bool AllowAllContentTypes { get; set; } = true;

    public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();

    // 20MB is the default
    // 0MB means no limit
    public long MaxAllowedFileSizeInBytes { get; set; } = 20 * 1024 * 1024;

    public double GetMaxAllowedSizeInMb()
    {
        if (MaxAllowedFileSizeInBytes <= 0)
        {
            return 0;
        }

        return Math.Round(MaxAllowedFileSizeInBytes / 1000000.0, 2);
    }
}
