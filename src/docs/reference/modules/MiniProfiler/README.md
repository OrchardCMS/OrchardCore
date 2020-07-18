# Mini Profiler (`OrchardCore.MiniProfiler`)

The module lets you use [Mini Profiler](https://miniprofiler.com/) to troubleshoot performance issues and generally to profile the performance of the application. Just enable the corresponding feature.

By default, the module will display the Mini Profiler performance widget on the frontend only. If you want to enable it for the admin too then use the `AllowMiniProfilerOnAdmin()` extension method to set the `MiniProfilerOptions.EnableOnAdmin` option (see the [documentation on configuration](../../core/Configuration/README.md)).