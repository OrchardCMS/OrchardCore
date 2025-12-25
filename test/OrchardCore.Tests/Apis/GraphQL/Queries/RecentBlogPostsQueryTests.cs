using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.GraphQL;

public class RecentBlogPostsQueryTests
{
    // Timing constants for handling concurrent background job race conditions
    private const int NestedJobSpawnDelayMs = 1000;  // Time to wait for jobs to spawn nested jobs
    private const int NestedJobWaitCycles = 3;        // Number of wait cycles for nested job completion
    private const int NestedJobCycleDelayMs = 300;    // Delay between nested job wait cycles
    private const int IndexPollingMaxAttempts = 15;   // Maximum attempts to poll for index results
    private const int IndexPollingDelayMs = 300;      // Delay between index polling attempts

    [Fact]
    public async Task ShouldListBlogPostWhenCallingAQuery()
    {
        using var context = new BlogContext();
        await context.InitializeAsync();

        // The recipe includes a RebuildIndex step which runs as a background job.
        // We need to ensure it completes before creating new content to avoid race conditions.
        await context.WaitForOutstandingDeferredTasksAsync(TestContext.Current.CancellationToken);
        await context.WaitForHttpBackgroundJobsAsync(TestContext.Current.CancellationToken);

        // Wait for any nested background jobs (SynchronizeAsync spawns additional jobs)
        await Task.Delay(NestedJobSpawnDelayMs, TestContext.Current.CancellationToken);
        await context.WaitForHttpBackgroundJobsAsync(TestContext.Current.CancellationToken);

        var blogPostContentItemId = await context
            .CreateContentItem("BlogPost", builder =>
            {
                builder.Published = true;
                builder.Latest = true;
                builder.DisplayText = "Some sort of blogpost in a Query!";

                builder
                    .Weld(new ContainedPart
                    {
                        ListContentItemId = context.BlogContentItemId,
                    });
            });

        // Wait for the new blog post to be indexed
        await context.WaitForOutstandingDeferredTasksAsync(TestContext.Current.CancellationToken);
        // Background jobs can spawn other background jobs, creating a window where ActiveJobsCount == 0
        // but a new job is about to be queued. We need to wait multiple times with delays to ensure
        // all nested jobs complete.
        for (var i = 0; i < NestedJobWaitCycles; i++)
        {
            await context.WaitForHttpBackgroundJobsAsync(TestContext.Current.CancellationToken);
            await Task.Delay(NestedJobCycleDelayMs, TestContext.Current.CancellationToken);
        }

        // Final wait to ensure all jobs are done
        await context.WaitForHttpBackgroundJobsAsync(TestContext.Current.CancellationToken);

        // Poll the index until both blog posts are available or timeout.
        // This handles any remaining race conditions in the indexing operations.
        JsonArray jsonArray = null;

        for (var attempt = 0; attempt < IndexPollingMaxAttempts; attempt++)
        {
            var result = await context
                .GraphQLClient
                .Content
                .Query("RecentBlogPosts", builder =>
                {
                    builder
                        .WithField("displayText")
                        .WithField("contentItemId");
                });

            jsonArray = result["data"]?["recentBlogPosts"]?.AsArray();

            if (jsonArray != null && jsonArray.Count == 2)
            {
                break;
            }

            // Wait before retrying
            await Task.Delay(IndexPollingDelayMs, TestContext.Current.CancellationToken);
        }

        Assert.NotNull(jsonArray);
        Assert.Equal(2, jsonArray.Count);

        // The RecentBlogPosts query sorts the content items by CreatedUtc. If the
        // test is executing too fast, both blog entries may have the same CreatedUtc
        // value and ordering becomes random. Because of this, we do not assert the order
        // of the result.
        var displayTexts = jsonArray.Select(node => node["displayText"]?.ToString());

        Assert.Contains("Some sort of blogpost in a Query!", displayTexts);

        // This is the blog post created by the default blog recipe.
        Assert.Contains("Man must explore, and this is exploration at its greatest", displayTexts);
    }
}
