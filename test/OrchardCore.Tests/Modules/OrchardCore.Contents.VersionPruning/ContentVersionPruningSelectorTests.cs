using OrchardCore.ContentManagement;
using OrchardCore.Contents.VersionPruning.Services;

namespace OrchardCore.Tests.Modules.Contents.VersionPruning;

public class ContentVersionPruningSelectorTests
{
    // ── no candidates ──────────────────────────────────────────────────────

    [Fact]
    public void SelectForDeletion_EmptyInput_ReturnsEmpty()
    {
        var result = ContentVersionPruningSelector.SelectForDeletion(new List<ContentItem>(), 1);
        Assert.Empty(result);
    }

    // ── single item, single version ────────────────────────────────────────

    [Fact]
    public void SelectForDeletion_SingleVersion_MinKeep1_ReturnsEmpty()
    {
        // Only one version exists for this item. MinVersionsToKeep=1 means keep it.
        var versions = new List<ContentItem>
        {
            MakeVersion("item-1", "v1", DateTime.UtcNow.AddDays(-40)),
        };

        var result = ContentVersionPruningSelector.SelectForDeletion(versions, minVersionsToKeep: 1);

        Assert.Empty(result);
    }

    [Fact]
    public void SelectForDeletion_SingleVersion_MinKeep0_WouldReturnIt_ButMinimumIs1AfterNormalize()
    {
        // The selector itself does not normalize; passing 0 means keep nothing.
        // In practice Normalize() is called first, so this tests the raw selector behaviour.
        var versions = new List<ContentItem>
        {
            MakeVersion("item-1", "v1", DateTime.UtcNow.AddDays(-40)),
        };

        var result = ContentVersionPruningSelector.SelectForDeletion(versions, minVersionsToKeep: 0);

        Assert.Single(result);
        Assert.Equal("v1", result[0].ContentItemVersionId);
    }

    // ── multiple versions for one item ─────────────────────────────────────

    [Fact]
    public void SelectForDeletion_ThreeVersions_MinKeep1_DeletesTwoOldest()
    {
        var now = DateTime.UtcNow;
        var versions = new List<ContentItem>
        {
            MakeVersion("item-1", "v1", now.AddDays(-90)),  // oldest
            MakeVersion("item-1", "v2", now.AddDays(-60)),
            MakeVersion("item-1", "v3", now.AddDays(-31)),  // most recent in the group
        };

        var result = ContentVersionPruningSelector.SelectForDeletion(versions, minVersionsToKeep: 1);

        // v3 must be kept (it is the 1 most-recent version to keep)
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, r => r.ContentItemVersionId == "v3");
        Assert.Contains(result, r => r.ContentItemVersionId == "v1");
        Assert.Contains(result, r => r.ContentItemVersionId == "v2");
    }

    [Fact]
    public void SelectForDeletion_ThreeVersions_MinKeep2_DeletesOnlyOldest()
    {
        var now = DateTime.UtcNow;
        var versions = new List<ContentItem>
        {
            MakeVersion("item-1", "v1", now.AddDays(-90)),
            MakeVersion("item-1", "v2", now.AddDays(-60)),
            MakeVersion("item-1", "v3", now.AddDays(-31)),
        };

        var result = ContentVersionPruningSelector.SelectForDeletion(versions, minVersionsToKeep: 2);

        Assert.Single(result);
        Assert.Equal("v1", result[0].ContentItemVersionId);
    }

    [Fact]
    public void SelectForDeletion_ThreeVersions_MinKeep3_DeletesNothing()
    {
        var now = DateTime.UtcNow;
        var versions = new List<ContentItem>
        {
            MakeVersion("item-1", "v1", now.AddDays(-90)),
            MakeVersion("item-1", "v2", now.AddDays(-60)),
            MakeVersion("item-1", "v3", now.AddDays(-31)),
        };

        var result = ContentVersionPruningSelector.SelectForDeletion(versions, minVersionsToKeep: 3);

        Assert.Empty(result);
    }

    // ── multiple items ─────────────────────────────────────────────────────

    [Fact]
    public void SelectForDeletion_TwoItems_EachKeepsMinVersions()
    {
        var now = DateTime.UtcNow;
        var versions = new List<ContentItem>
        {
            // item-A: 3 versions, keep 1 → delete 2
            MakeVersion("item-A", "A-v1", now.AddDays(-120)),
            MakeVersion("item-A", "A-v2", now.AddDays(-90)),
            MakeVersion("item-A", "A-v3", now.AddDays(-40)),
            // item-B: 2 versions, keep 1 → delete 1
            MakeVersion("item-B", "B-v1", now.AddDays(-80)),
            MakeVersion("item-B", "B-v2", now.AddDays(-35)),
        };

        var result = ContentVersionPruningSelector.SelectForDeletion(versions, minVersionsToKeep: 1);

        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, r => r.ContentItemVersionId == "A-v3");
        Assert.DoesNotContain(result, r => r.ContentItemVersionId == "B-v2");
    }

    // ── Latest / Published guard ───────────────────────────────────────────

    [Fact]
    public void SelectForDeletion_NeverSetsLatestOrPublished_OnResults()
    {
        // Verify that all output items have Latest=false and Published=false.
        var now = DateTime.UtcNow;
        var versions = new List<ContentItem>
        {
            MakeVersion("item-1", "v1", now.AddDays(-90)),
            MakeVersion("item-1", "v2", now.AddDays(-60)),
            MakeVersion("item-1", "v3", now.AddDays(-31)),
        };

        var result = ContentVersionPruningSelector.SelectForDeletion(versions, minVersionsToKeep: 1);

        Assert.All(result, r =>
        {
            Assert.False(r.Latest);
            Assert.False(r.Published);
        });
    }

    [Fact]
    public void SelectForDeletion_FiltersOutLatestVersion()
    {
        // The selector guards against Latest versions even when mistakenly passed in.
        var now = DateTime.UtcNow;
        var latestVersion = new ContentItem
        {
            ContentItemId = "item-1",
            ContentItemVersionId = "v-latest",
            ContentType = "TestPage",
            ModifiedUtc = now.AddDays(-40),
            Latest = true,
            Published = false,
        };
        var oldVersion = MakeVersion("item-1", "v-old", now.AddDays(-90));

        var versions = new List<ContentItem> { latestVersion, oldVersion };

        var result = ContentVersionPruningSelector.SelectForDeletion(versions, minVersionsToKeep: 0);

        Assert.Single(result);
        Assert.Equal("v-old", result[0].ContentItemVersionId);
    }

    [Fact]
    public void SelectForDeletion_FiltersOutPublishedVersion()
    {
        // The selector guards against Published versions even when mistakenly passed in.
        var now = DateTime.UtcNow;
        var publishedVersion = new ContentItem
        {
            ContentItemId = "item-1",
            ContentItemVersionId = "v-published",
            ContentType = "TestPage",
            ModifiedUtc = now.AddDays(-40),
            Latest = false,
            Published = true,
        };
        var oldVersion = MakeVersion("item-1", "v-old", now.AddDays(-90));

        var versions = new List<ContentItem> { publishedVersion, oldVersion };

        var result = ContentVersionPruningSelector.SelectForDeletion(versions, minVersionsToKeep: 0);

        Assert.Single(result);
        Assert.Equal("v-old", result[0].ContentItemVersionId);
    }

    [Fact]
    public void SelectForDeletion_AllVersionsLatestOrPublished_ReturnsEmpty()
    {
        // If every version in a group is Latest or Published the result should be empty.
        var now = DateTime.UtcNow;
        var versions = new List<ContentItem>
        {
            new()
            {
                ContentItemId = "item-1",
                ContentItemVersionId = "v1",
                ContentType = "TestPage",
                ModifiedUtc = now.AddDays(-90),
                Latest = true,
                Published = false,
            },
            new()
            {
                ContentItemId = "item-1",
                ContentItemVersionId = "v2",
                ContentType = "TestPage",
                ModifiedUtc = now.AddDays(-40),
                Latest = false,
                Published = true,
            },
        };

        var result = ContentVersionPruningSelector.SelectForDeletion(versions, minVersionsToKeep: 0);

        Assert.Empty(result);
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private static ContentItem MakeVersion(string contentItemId, string versionId, DateTime? modifiedUtc)
        => new ContentItem
        {
            ContentItemId = contentItemId,
            ContentItemVersionId = versionId,
            ContentType = "TestPage",
            DisplayText = $"{contentItemId} – {versionId}",
            ModifiedUtc = modifiedUtc,
            Latest = false,
            Published = false,
        };
}
