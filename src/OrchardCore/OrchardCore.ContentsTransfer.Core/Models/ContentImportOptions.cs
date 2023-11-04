using System;

namespace OrchardCore.ContentsTransfer.Models;

public class ContentImportOptions
{
    public const int AbsoluteMaxAllowedFileSizeInBytes = 100;

    public bool AllowAllContentTypes { get; set; } = true;

    public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();

    // 20MB is the default
    public long MaxAllowedFileSizeInBytes { get; set; } = 20 * 1024 * 1024;

    public double GetMaxAllowedSizeInMb()
    {
        if (MaxAllowedFileSizeInBytes < 1)
        {
            return AbsoluteMaxAllowedFileSizeInBytes;
        }

        var configuredSizeInMb = Math.Round(MaxAllowedFileSizeInBytes / 1000000.0, 2);

        return Math.Min(configuredSizeInMb, AbsoluteMaxAllowedFileSizeInBytes);
    }
}
