using OrchardCore.ContentManagement;
using OrchardCore.Contents.VersionPruning.Services;

namespace OrchardCore.Tests.Modules.Contents.VersionPruning;

public class ContentVersionPruningSelectorTests
{
    private static readonly DateTime s_now = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // A 30-day retention threshold relative to s_now.
    private static readonly DateTime s_threshold = s_now.AddDays(-30);

    [Fact]
    public void SelectForDeletion_EmptyInput_ReturnsEmpty()
    {
        var result = ContentVersionPruningSelector.SelectForDeletion([], versionsToKeep: 1, s_threshold);

        Assert.Empty(result);
    }

    [Fact]
    public void SelectForDeletion_SingleVersionKeep1_ReturnsEmpty()
    {
        var versions = new List<ContentItem>
        {
            MakeVersion("item-1", "v1", s_now.AddDays(-40)),
        };

        var result = ContentVersionPruningSelector.SelectForDeletion(versions, versionsToKeep: 1, s_threshold);

        Assert.Empty(result);
    }

    [Fact]
    public void SelectForDeletion_KeepNewest_DeletesOnlyAgedRemainder()
    {
        // The most-recent archived version (v3) is kept because of VersionsToKeep=1.
        // Of the remaining versions, only those older than the threshold are deleted.
        var versions = new List<ContentItem>
        {
            MakeVersion("item-1", "v3", s_now.AddDays(-5)),   // newest, kept by VersionsToKeep
            MakeVersion("item-1", "v2", s_now.AddDays(-10)),  // not older than threshold, kept
            MakeVersion("item-1", "v1", s_now.AddDays(-90)),  // older than threshold, deleted
        };

        var result = ContentVersionPruningSelector.SelectForDeletion(versions, versionsToKeep: 1, s_threshold);

        Assert.Single(result);
        Assert.Equal("v1", result[0].ContentItemVersionId);
    }

    [Fact]
    public void SelectForDeletion_AgeBeatsKeep_OldVersionDeletedWhenNewerArchivedSatisfiesQuota()
    {
        // Regression for the age/keep interaction: an old archived version must be deletable
        // when newer archived versions already satisfy the "keep N newest" quota,
        // even though the old version is itself the only one past the threshold.
        var versions = new List<ContentItem>
        {
            MakeVersion("item-1", "v3", s_now.AddDays(-5)),
            MakeVersion("item-1", "v2", s_now.AddDays(-10)),
            MakeVersion("item-1", "v1", s_now.AddDays(-90)),
        };

        var result = ContentVersionPruningSelector.SelectForDeletion(versions, versionsToKeep: 1, s_threshold);

        Assert.Contains(result, r => r.ContentItemVersionId == "v1");
        Assert.DoesNotContain(result, r => r.ContentItemVersionId == "v3");
        Assert.DoesNotContain(result, r => r.ContentItemVersionId == "v2");
    }

    [Fact]
    public void SelectForDeletion_Keep0_DeletesAllAgedVersions()
    {
        var versions = new List<ContentItem>
        {
            MakeVersion("item-1", "v1", s_now.AddDays(-90)),
            MakeVersion("item-1", "v2", s_now.AddDays(-60)),
            MakeVersion("item-1", "v3", s_now.AddDays(-31)),
        };

        var result = ContentVersionPruningSelector.SelectForDeletion(versions, versionsToKeep: 0, s_threshold);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void SelectForDeletion_KeepBeyondCount_DeletesNothing()
    {
        var versions = new List<ContentItem>
        {
            MakeVersion("item-1", "v1", s_now.AddDays(-90)),
            MakeVersion("item-1", "v2", s_now.AddDays(-60)),
        };

        var result = ContentVersionPruningSelector.SelectForDeletion(versions, versionsToKeep: 5, s_threshold);

        Assert.Empty(result);
    }

    [Fact]
    public void SelectForDeletion_NullModifiedUtc_TreatedAsOldest()
    {
        // A null ModifiedUtc sorts last (oldest) and is always considered past the threshold.
        var versions = new List<ContentItem>
        {
            MakeVersion("item-1", "v2", s_now.AddDays(-5)),
            MakeVersion("item-1", "v1", modifiedUtc: null),
        };

        var result = ContentVersionPruningSelector.SelectForDeletion(versions, versionsToKeep: 1, s_threshold);

        Assert.Single(result);
        Assert.Equal("v1", result[0].ContentItemVersionId);
    }

    [Fact]
    public void SelectForDeletion_FiltersOutLatestAndPublished_Succeeds()
    {
        var versions = new List<ContentItem>
        {
            new()
            {
                ContentItemId = "item-1",
                ContentItemVersionId = "v-latest",
                ContentType = "TestPage",
                ModifiedUtc = s_now.AddDays(-40),
                Latest = true,
            },
            new()
            {
                ContentItemId = "item-1",
                ContentItemVersionId = "v-published",
                ContentType = "TestPage",
                ModifiedUtc = s_now.AddDays(-50),
                Published = true,
            },
            MakeVersion("item-1", "v-old", s_now.AddDays(-90)),
        };

        var result = ContentVersionPruningSelector.SelectForDeletion(versions, versionsToKeep: 0, s_threshold);

        Assert.Single(result);
        Assert.Equal("v-old", result[0].ContentItemVersionId);
    }

    private static ContentItem MakeVersion(string contentItemId, string versionId, DateTime? modifiedUtc)
        => new()
        {
            ContentItemId = contentItemId,
            ContentItemVersionId = versionId,
            ContentType = "TestPage",
            DisplayText = $"{contentItemId} - {versionId}",
            ModifiedUtc = modifiedUtc,
            Latest = false,
            Published = false,
        };
}
