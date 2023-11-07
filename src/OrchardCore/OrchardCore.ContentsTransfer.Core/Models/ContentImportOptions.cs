using System;

namespace OrchardCore.ContentsTransfer.Models;

public class ContentImportOptions
{
    // 100MB is the absolute max.
    public const int AbsoluteMaxAllowedFileSizeInBytes = 100 * 1024 * 1024;

    public bool AllowAllContentTypes { get; set; } = true;

    public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();

    // 20MB is the default.
    public long MaxAllowedFileSizeInBytes { get; set; } = 20 * 1024 * 1024;

    public long GetMaxAllowedSize()
    {
        if (MaxAllowedFileSizeInBytes < 1)
        {
            return AbsoluteMaxAllowedFileSizeInBytes;
        }

        return Math.Min(MaxAllowedFileSizeInBytes, AbsoluteMaxAllowedFileSizeInBytes);
    }
}
