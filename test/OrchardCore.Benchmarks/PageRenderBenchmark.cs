using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using OrchardCore.Admin;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Benchmarks;

/// <summary>
/// Renders real pages of a Blog recipe tenant through the test server, and reports how many shapes each page
/// renders and through which bindings. Run with <c>--profiler EP</c> to collect a CPU sampling trace of the
/// requests, then <c>dotnet-trace report &lt;file&gt; topN</c> to see where the time goes.
/// </summary>
[MemoryDiagnoser]
public class PageRenderBenchmark
{
    private BlogContext _context;
    private HttpClient _client;
    private string _blogPostPath;
    private string _blogPostEditPath;

    private static long s_shapesRendered;

    [GlobalSetup]
    public async Task SetupAsync()
    {
        _context = new BlogContext
        {
            // Every permission requirement succeeds, so the admin pages render without a login.
            PermissionsContext = new PermissionsContext
            {
                UsePermissionsContext = false,
                AuthorizedPermissions = [AdminPermissions.AccessAdminPanel, CommonPermissions.EditContent, CommonPermissions.ListContent],
            },
        };

        await _context.InitializeAsync();
        _client = _context.Client;

        var home = await GetAsync("");
        _blogPostPath = Regex.Match(home, "href=\"/[^\"]*/blog/([^\"]+)\"").Groups[1].Value;
        _blogPostPath = "blog/" + _blogPostPath;

        _blogPostEditPath = $"Admin/Contents/ContentItems/{_context.BlogContentItemId}/Edit";

        using var meterListener = new MeterListener();
        meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == DisplayManagementDiagnostics.MeterName && instrument.Name == "orchardcore.display.shapes_rendered")
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        meterListener.SetMeasurementEventCallback<long>((_, value, _, _) => Interlocked.Add(ref s_shapesRendered, value));
        meterListener.Start();

        var bindings = new Dictionary<string, int>();
        var types = new Dictionary<string, int>();
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == DisplayManagementDiagnostics.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = activity =>
            {
                var type = activity.GetTagItem("orchardcore.shape.type")?.ToString() ?? "?";
                var binding = activity.GetTagItem("orchardcore.shape.binding")?.ToString() ?? "(no binding: cached or wrapper only)";
                var engine = binding.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase) ? "razor"
                    : binding.EndsWith(".liquid", StringComparison.OrdinalIgnoreCase) ? "liquid"
                    : binding.Contains("::") ? "code" : binding;

                lock (bindings)
                {
                    bindings[engine] = bindings.GetValueOrDefault(engine) + 1;
                    types[type] = types.GetValueOrDefault(type) + 1;
                }
            },
        };
        ActivitySource.AddActivityListener(activityListener);

        foreach (var (name, path) in new[] { ("Home", ""), ("BlogPost", _blogPostPath), ("AdminContentList", "Admin/Contents/ContentItems"), ("AdminEditBlog", _blogPostEditPath) })
        {
            bindings.Clear();
            types.Clear();
            var before = Interlocked.Read(ref s_shapesRendered);
            var html = await GetAsync(path);
            var count = Interlocked.Read(ref s_shapesRendered) - before;

            Console.WriteLine($"// {name}: {count} shapes rendered, {html.Length:N0} chars of HTML");
            Console.WriteLine($"//   by engine: {string.Join(", ", bindings.OrderByDescending(x => x.Value).Select(x => $"{x.Key}={x.Value}"))}");
            Console.WriteLine($"//   top types: {string.Join(", ", types.OrderByDescending(x => x.Value).Take(12).Select(x => $"{x.Key}={x.Value}"))}");
        }
    }

    [GlobalCleanup]
    public void Cleanup() => _context?.Dispose();

    private async Task<string> GetAsync(string path)
    {
        using var response = await _client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [Benchmark(Description = "GET / (blog home, list of posts)")]
    public Task<string> Home() => GetAsync("");

    [Benchmark(Description = "GET /blog/{post} (detail)")]
    public Task<string> BlogPost() => GetAsync(_blogPostPath);

    [Benchmark(Description = "GET /Admin/Contents/ContentItems (admin list)")]
    public Task<string> AdminContentList() => GetAsync("Admin/Contents/ContentItems");

    [Benchmark(Description = "GET /Admin/Contents/ContentItems/{id}/Edit (editor)")]
    public Task<string> AdminEditBlog() => GetAsync(_blogPostEditPath);
}
