using System;
using OrchardCore.Entities;

namespace OrchardCore.ContentTransfer;

public class ContentTransferEntry : Entity
{
    /// <summary>
    /// The primary key in the database.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The logical identifier of the entry.
    /// </summary>
    public string EntryId { get; set; }

    /// <summary>
    /// The content type being imported.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// When the entry was been created or first created.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// When the entry was last processed.
    /// </summary>
    public DateTime? ProcessSaveUtc { get; set; }

    /// <summary>
    /// When the entry finished processing.
    /// </summary>
    public DateTime? CompletedUtc { get; set; }

    /// <summary>
    /// The user id of the user who created this entry.
    /// </summary>
    public string Owner { get; set; }

    /// <summary>
    /// The Original file name.
    /// </summary>
    public string UploadedFileName { get; set; }

    /// <summary>
    /// The stored file name.
    /// </summary>
    public string StoredFileName { get; set; }

    public ContentTransferEntryStatus Status { get; set; }

    /// <summary>
    /// Error message if the file failed processing.
    /// </summary>
    public string Error { get; set; }
}

public enum ContentTransferEntryStatus
{
    New = 1,
    Processing = 2,
    Completed = 3,
    CompletedWithErrors = 4,
    Failed = 5,
}
