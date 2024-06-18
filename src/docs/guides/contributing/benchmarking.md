# Benchmarking

To measure how fast Orchard Core is (usually, it's pretty fast :)), we employ some benchmarking. You can help extend the benchmarking suite too! (See the [code contribution guidelines](contributing-code.md).)

## `OrchardCore.Benchmarks`

In the [`OrchardCore.Benchmarks` project](https://github.com/OrchardCMS/OrchardCore/tree/main/test/OrchardCore.Benchmarks) we have several benchmarks created with [BenchmarkDotNet](https://benchmarkdotnet.org/). When we troubleshoot some performance issues, we extend these to measure the performance impact of the changes.

### Running `OrchardCore.Benchmarks` benchmarks

The project is a console application. Build the whole solution in Release mode, then run `OrchardCore.Benchmarks` (it'll be under `test/OrchardCore.Benchmarks/bin/Release/net8.0/OrchardCore.Benchmarks.exe`): You'll see a CLI menu to select which benchmark to run. Alternatively, you can use one of BenchmarkDotNet's [console arguments](https://benchmarkdotnet.org/articles/guides/console-args.html).

## ASP.NET Core Benchmarks

The [ASP.NET Core Benchmarks](https://github.com/aspnet/Benchmarks) also include some continuous benchmarks of Orchard Core. The results can be seen under <https://aka.ms/aspnet/benchmarks>, among the MVC benchmarks (select "MVC" from the menu by clicking the page numbers at the bottom; direct link [here](https://msit.powerbi.com/view?r=eyJrIjoiYTZjMTk3YjEtMzQ3Yi00NTI5LTg5ZDItNmUyMGRlOTkwMGRlIiwidCI6IjcyZjk4OGJmLTg2ZjEtNDFhZi05MWFiLTJkN2NkMDExZGI0NyIsImMiOjV9&pageName=ReportSection36a3b7283aa365d8de32)).