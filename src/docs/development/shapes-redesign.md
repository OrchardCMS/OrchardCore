# Shapes: a lighter design

A proposal to redesign the shape system of Orchard Core so that it stops being a per-fragment tax on every page, while keeping what it is for: letting modules, themes and the application compose and override the UI without knowing about each other. Breaking changes are on the table; the proposal says which ones are worth it, and which ones aren't.

**Summary.** The shape pipeline costs about 2.5 µs and 1.8 KB per shape before any template runs, which a prototype of the proposed model brings down 20 times in CPU and 10 times in memory. But on the pages measured, the pipeline itself is a few percent of a request: what makes a page of shapes expensive is what each shape drags along, the MVC partial view rendered for every Razor template, the dynamic model, the per-shape buffers and copies, the per-tenant shape table, and the allocation volume (1.5 MB for a 7 KB page). The proposal therefore changes the shape *model* (typed nodes, shared alternates, cached resolution, streaming rendering) so that these costs can be removed, rather than optimizing the current objects further. Theme authors keep their conventions; module authors get typed, smaller APIs; the `dynamic` surface goes away.

The numbers in this document come from the benchmarks added with it in `test/OrchardCore.Benchmarks` (`ShapePipelineBenchmark`, `DynamicShapeAccessBenchmark`, `ShapePrototypeBenchmark`, `PageRenderBenchmark`, `ShapeTemplateEngineBenchmark`). The prototype of the proposed model lives in `test/OrchardCore.Benchmarks/ShapePrototype`.

## 1. What shapes are for

Before changing the implementation, this is what the system must keep providing. Everything else is implementation detail and can go.

| Capability | Who uses it | What depends on it |
| --- | --- | --- |
| **Composition**: a page is a tree of fragments that many independent modules contribute to, each one into a named zone, at a position. | Every display driver (557 of them in the repository), the layout, widgets, the admin UI. | Drivers, `placement.json`, the Placements module, zones in layouts. |
| **Theming by convention**: a theme or the application overrides a fragment by dropping a file whose name matches a shape type or one of its alternates, with the precedence application > theme > base themes > modules. | Every theme author. It is the documented way to customize Orchard Core, in Razor and in Liquid. | 1,900 `.cshtml` and 57 `.liquid` templates in the repository, the `Templates` module (templates stored in the database), the `Templates and Alternates` documentation. |
| **Placement without code**: move, hide, reposition, wrap, or re-type a fragment, per display type, differentiator, content type or path. | Site builders, themes (`placement.json`), administrators (Placements module). | `placement.json` syntax, tabs/cards/columns grouping in editors. |
| **Display types**: the same content renders differently as `Detail`, `Summary`, `SummaryAdmin`, `Edit`, etc. | Lists, feeds, admin lists, editors. | Drivers' `Location("Summary", ...)`, display type alternates. |
| **Fragment caching**: a fragment declares a cache id, contexts and tags, and its rendered HTML is reused across requests. | Dynamic Cache module, `<shape cache-id>` and `{% shape cache_id %}`. | Needs the pipeline to be able to skip rendering a subtree and inject HTML. |
| **Editors from the same model**: an editor is a tree of editor fragments built by the same drivers, with HTML field prefixes and model binding. | Every content editor and settings page. | `BuildEditorAsync` / `UpdateEditorAsync`, `Prefix`, `ocat-*` admin edit views. |
| **Code fragments**: a fragment rendered by a method (`[Shape]`) or a delegate, not a file. | Zones, grouping containers, pagers, date/time formats, 36 `[Shape]` methods. | `IShapeAttributeProvider`, `BoundAs()`. |

Two things are *not* on the list, although the current implementation is built around them:

- **Dynamic objects**. `Shape : Composite : DynamicObject`, `New.Foo()`, `Model.Foo` without `@model`, `Nil`, `ZoneOnDemand`. They were the C# 4 way of giving templates a flexible model; today 1,199 of the 1,887 templates in `Views` folders declare `@model`, 453 driver results use `Initialize<TModel>`, and the open issue [#18641](https://github.com/OrchardCMS/OrchardCore/issues/18641) is already about removing the remaining dynamic shapes.
- **One object for everything**. The same `IShape` instance is the model handed to the template, the node of the tree, the carrier of rendering metadata, the target of six lifecycle events and the holder of its own rendered output (`Metadata.ChildContent`). Nothing in the capabilities above requires that.

## 2. What a shape costs today

### 2.1 The pipeline per shape

`ShapePipelineBenchmark` renders shapes whose binding is a code delegate returning a constant string, so the numbers are the cost of the pipeline alone (`DefaultShapeFactory`, `ShapeResult`, `DefaultHtmlDisplay`), without any template engine. The floor is the delegate called directly.

| Scenario (current pipeline, code bindings) | Mean | Allocated |
| --- | ---: | ---: |
| Floor: call the binding delegate directly | 30 ns | 72 B |
| Create: dynamic `Shape` through `IShapeFactory` | 174 ns | 576 B |
| Create: typed proxy (`Initialize<TModel>`) through `IShapeFactory` | 201 ns | 584 B |
| Create: `new ShapeViewModel()` (no factory) | 38 ns | 296 B |
| Render leaf: no alternates | 1,215 ns | 928 B |
| Render leaf: 5 alternates, none bound | 1,891 ns | 1,440 B |
| Render leaf: 5 alternates + `Templates` resolver | 1,902 ns | 1,440 B |
| Render leaf: 5 alternates + 1 wrapper | 2,358 ns | 2,072 B |
| Place: `ShapeResult.ApplyAsync()` (`Initialize<TModel>` + placement rules) | 1,164 ns | 688 B |
| Content tree: build 6 results and render 9 shapes | 22,541 ns | 16,160 B |

*.NET 10, x64, 4 cores, in-process short run (3 warmups, 3 iterations); the content tree row has a ±4 µs standard deviation.*

What the table says:

- Rendering a leaf shape through `DefaultHtmlDisplay` costs 40 times the direct call and allocates 928 bytes, for a shape that has no alternates, no wrapper, no event and no cache. That is the fixed price of `DisplayContext` + `ShapeDisplayContext` + the event loops + the `Activity`/`Stopwatch` checks + the binding lookup + the `IHtmlContent` result.
- A typical content part shape carries 5 alternates (see 2.3). None of them usually has a template, so they are 5 dictionary misses before the base type is found, on every shape, on every request: +0.7 µs and +512 bytes per shape, the latter being the `OrderedDictionary` that holds them.
- With the `Templates` feature enabled, an `IShapeBindingResolver` is asked for each alternate and for the type, as an `async` interface call each. The resolver answers from a per-request cache, so the six calls add little here (+10 ns); the first call of a request loads the templates document.
- A wrapper adds +0.5 µs and +632 bytes: a second binding lookup and a second `IHtmlContent` buffer around the first.
- Placing one driver result (`Initialize<TModel>()` + `Location()` + `ShapeResult.ApplyAsync()`) costs 1.2 µs and 688 bytes: a `ShapeResult`, a closure state tuple, the placement predicates chain, a `ShapePlacementContext`, a `PlacementInfo`, the Castle DynamicProxy instance of the model (the proxy alone is about 230 ns and 700 bytes, see `ShapeProxyBenchmark`), and the alternates copied into the shape.
- A content item with six parts and fields (nine shapes, two zones) costs 22.5 µs and 16 KB to build and render, before any template runs: about 2.5 µs and 1.8 KB per shape.

### 2.2 Reading data from a template

`DynamicShapeAccessBenchmark`, in nanoseconds:

| Access | Mean | Ratio |
| --- | ---: | ---: |
| Typed property | 0.3 ns | 1 |
| `IShape.Properties["Title"]` | 5.3 ns | 18 |
| `dynamic Model.Title` (DLR) | 15.7 ns | 53 |
| `dynamic Model.Missing` (DLR, falls back to `Named()` + `NormalizedNamed()`) | 31.6 ns | 106 |
| `dynamic Layout.Header` (`ZoneHolding`, zone created on demand) | 18.5 ns | 62 |
| `dynamic Model.Title = value` (DLR set) | 21.0 ns | 70 |

Each is small, but a template reads a dozen members, a page renders hundreds of shapes, and the DLR call sites are per template and per member, so they also cost memory and startup JIT. The `Missing` row is the common `@if (Model.Footer != null)` idiom: a miss on the properties, then two linear scans of the children by name. On a `ZoneHolding` (the `Layout` and every content shape) the same idiom *allocates* a `ZoneOnDemand` and stores it in the properties the first time each absent zone is read on each shape, so that `Model.Footer` can be compared to `null` and still accept `AddAsync()`. The benchmark reuses one layout so the row shows no allocation; on a page every content shape is new, and `Content.cshtml` alone reads three zones that are usually absent.

### 2.3 Alternates are speculative

Alternates exist so that a theme can override a fragment at a finer grain than its type. They are generated eagerly, as strings, on every shape, for every grain the convention allows, whether or not any template exists for it:

| Shape | Alternates added per instance | By |
| --- | ---: | --- |
| `Content` (a content item) | 5, two of them unique per content item id (`Content__42`) | `Shapes.OnDisplaying` in `OrchardCore.Contents` |
| A content part (`TitlePart`) | 5 to 9 | `ContentPartDisplayDriver<T>.Factory()` → `ContentPartAlternatesFactory` |
| A content field (`TextField`) | 6 to 12 | `ContentFieldDisplayDriver<T>` → `ContentFieldAlternatesFactory` |
| A widget, a menu item, a term, a pager link | 2 to 6 | Their shape table providers |

The factories already cache the generated `string[]` per convention key, which is good. But each shape then copies them into its own `AlternatesCollection` (an `OrderedDictionary<string, string>`), and the renderer probes each one against the shape table, every time. In the Blog theme, 29 templates exist for the thousands of alternate names generated per page. The resolution is a pure function of (shape table, type, alternates), and the alternates are a pure function of a few strings (content type, part, field, display type), so it can be computed once per distinct combination per tenant, not once per shape.

The per-content-item-id alternates (`Content__{id}`, `Content_{displayType}__{id}`) defeat any such cache and are rarely used; they should be opt-in.

### 2.4 End-to-end pages

`PageRenderBenchmark` renders real pages of a `Blog` recipe tenant through the test server and counts the shapes with the `OrchardCore.DisplayManagement` meter and activity source:

| Page (Blog recipe, test server, Sqlite) | Shapes rendered | By engine | Request time | Allocated per request | HTML |
| --- | ---: | --- | ---: | ---: | ---: |
| `GET /` (blog home, list of posts) | 19 | 8 code, 5 Razor, 5 Liquid | 11.0 ms | 1,453 KB | 7.6 KB |
| `GET /blog/{post}` (detail, with widgets) | 15 | 5 code, 5 Liquid, 4 Razor | 9.0 ms | 1,509 KB | 8.5 KB |
| `GET /Admin/Contents/ContentItems` (admin list, 5 items) | 148 | 83 Razor, 61 code | 8.7 ms | 1,699 KB | 48 KB |
| `GET /Admin/Contents/ContentItems/{id}/Edit` (editor) | 64 | 47 Razor, 17 code | 5.7 ms | 919 KB | 36 KB |

*In-process short run; the absolute times include the test server and the console logging of the test host, and are only comparable to each other.*

Two things stand out:

- **The shape pipeline is a small share of these requests.** At 2.5 µs and 1.8 KB per shape (2.1), the pipeline accounts for about 50 µs and 35 KB of the home page (0.5% of the time, 2% of the memory) and about 370 µs and 270 KB of the admin list (4% and 16%). A CPU sampling trace of the run (`dotnet-trace`, see the benchmark's remarks) agrees: the frames of `DefaultHtmlDisplay`, `ShapeResult` and `DefaultShapeFactory` hold a few percent of the request samples. The rest of a request is the MVC pipeline, DI scopes, YesSql and Sqlite, the Razor views and tag helpers, and the GC work induced by the allocation volume.
- **The allocation volume is the problem, and shapes multiply it through the template engines.** 1.5 MB per request for 7 KB of HTML is 200 bytes allocated per byte of output. The admin list, with 148 shapes of which 83 are Razor templates, allocates the most. What each engine costs per shape, in the same tenant, is measured in 2.5.

The trace also shows two costs that are not per request:

- **Cold start per tenant**: harvesting the templates of all the enabled extensions (`ShapeTemplateBindingStrategy`), reflecting over the `[Shape]` methods and building the descriptors, plus the first Razor view lookups, the Liquid template parsing, the DI call sites and the JIT, were 20% of the busy CPU of the whole run (16 s for the three tenants the benchmark created after the trace started, and their first renders of four pages). Every new tenant, and every theme switch, pays the shape table part again.
- **Runtime expression compilation on first use**: `RazorPageActivator`, `ViewDataDictionaryFactory` and the DLR call sites compile expression trees the first time a page type, a model type or a dynamic member access is seen, per tenant, since each tenant has its own service provider.

### 2.5 The template engines per shape

`ShapeTemplateEngineBenchmark` renders one shape 100 times through `IShapeHtmlRenderer` inside the Blog tenant: a code binding (the `DateTime` shape), the smallest Razor template of the repository (`TitlePart.cshtml`, `<h1>@Model.Title</h1>`, typed model) and a Liquid template of the theme (`Search-Form.liquid`).

{{ENGINES_TABLE}}

### 2.6 Structural costs that benchmarks don't show

- **The shape table is built per tenant and per theme on the first request**, by enumerating the `Views`, `Views/Items`, `Views/Parts`, `Views/Fields`, `Views/Elements` folders of every enabled extension through the file provider, and by reflecting over every `[Shape]` method. Descriptors are shared across tenants, but the enumeration and the `ShapeDescriptorIndex` construction are per tenant, and the cold start of a tenant pays it.
- **Castle DynamicProxy** generates a proxy type per view model type, at runtime, and every typed shape is a proxy with a `ShapeViewModel` mixin. It is incompatible with native AOT and trimming, and it forces view models to be non-sealed with virtual-friendly members.
- **`Items` are sorted by re-parsing position strings** (`FlatPositionComparer` splits `"5.1"` on each comparison) on first enumeration, with an `OrderBy().ToList()` allocation per container.
- **Six lifecycle events** (`Creating`, `Created`, `Displaying`, `Processing`, `Displayed`, `DisplayingFinalized`) at three scopes (instance, type, global) are checked for every shape. Across the repository: 54 `OnDisplaying` (almost all add alternates), 14 `OnProcessing` (lazy data loading, so that the Dynamic Cache can skip it), 6 `OnCreated`, 4 `OnDisplayed`, 3 `OnCreating`, and two global `IShapeDisplayEvents` (Dynamic Cache, MiniProfiler).
- **`IHtmlContent` per shape**: each template renders into its own `ViewBuffer`, returned as `IHtmlContent`, stored in `Metadata.ChildContent`, then copied by the parent template into its own buffer. A page is copied once per level of the tree. The Dynamic Cache then renders the buffer to a string a second time to store it.
- **Razor shape templates render as MVC partials**: `IHtmlHelper.PartialAsync()` → `ViewEngine.FindView()` (cached by name and theme) → `RazorView.RenderAsync()` with a new `ViewDataDictionary`, a new `ViewContext`, a page activation and a `ViewBuffer`. The Liquid engine already bypasses `RazorView` and renders the parsed Fluid template directly, for this reason.

## 3. Goals

1. **The pipeline cost per fragment is a dictionary lookup and a virtual call.** No context objects, no event loops, no string probing, no proxy, no intermediate buffer, in the common case.
2. **Theme authors see no difference.** Same file naming conventions, same `placement.json`, same alternates vocabulary, same precedence, Razor and Liquid.
3. **Module authors write less, typed, code.** A driver returns a model and a location. A template declares its model type. The compiler checks both.
4. **Rendering streams.** A fragment writes to the response, wrappers wrap, the cache captures only what it caches.
5. **Startup and memory**: no runtime code generation, no per-shape metadata dictionaries, resolution caches bounded by the number of conventions, not by the number of shapes.
6. **AOT and trimming friendly**: no `dynamic`, no Castle, no reflection in the hot path.

Non-goals: changing what themes can override, replacing Razor or Liquid, changing `placement.json`, merging display types into something else, or changing how content editors are assembled.

## 4. The proposed model

The shape is split into the three things it conflates today.

```text
                ┌──────────────────────────┐
  driver  ───►  │ ShapeNode                │  ◄── placement (zone, position, alternates, wrappers)
                │  Type, DisplayType, Name │
                │  Model: object           │  ◄── any CLR object, the template's @model
                │  Alternates: AlternateSet│  ◄── shared, cached per convention key
                │  Zones / Children        │
                └────────────┬─────────────┘
                             │ ShapeKey (Type + AlternateSet refs)
                             ▼
                ┌──────────────────────────┐
                │ RenderPlan (per key,     │  ◄── resolved once per tenant/theme
                │  cached in TemplateTable)│
                │  Template, Wrappers[]    │
                └────────────┬─────────────┘
                             │
                             ▼
                IShapeTemplate.RenderAsync(ctx) ──► writes to the output
```

### 4.1 `ShapeNode`: the tree, nothing else

```csharp
public sealed class ShapeNode
{
    public string Type { get; set; }               // "TitlePart"
    public string DisplayType { get; set; }        // "Summary"
    public string Name { get; set; }               // "TitlePart", "Header"
    public string Differentiator { get; set; }
    public string Prefix { get; set; }             // editors
    public object Model { get; set; }              // the view model
    public AlternateSet Alternates { get; set; }   // shared, see 4.2
    public AlternateSet PlacementAlternates { get; set; }
    public string[] Wrappers { get; set; }         // rare, usually null
    public Position Position { get; }              // parsed once
    public GroupingMetadata Tab, Card, Column;     // editors
    public CacheContext Cache { get; set; }        // null unless cached

    public IReadOnlyList<ShapeNode> Children { get; }    // sorted on read
    public ShapeNode GetZone(string name);                // null when absent, no allocation
    public ShapeNode GetOrAddZone(string name);
    public void Add(ShapeNode child, Position position);
}
```

- It is **not dynamic**, not an `IShape`, and not the model. `Model` is any object: a view model, a `ContentItem`, a string. `ShapeNode` itself has no `Properties` bag; a template that needs ad hoc values uses a `Dictionary<string, object>` model, or a record.
- **Zones are a small array of named child nodes**, not properties. `GetZone("Footer")` returns `null` when the zone was never filled; nothing is allocated by asking. `Layout` and content shapes are plain `ShapeNode`s whose zones are nodes of type `Zone`.
- **Positions are parsed when the child is added**, into a comparable value (segments, `before`/`after`/`start`/`end` tiers), so sorting compares integers.
- `Children` is the only ordered collection; `Items`, `Named()`, `NormalizedNamed()` and `Remove(name)` are replaced by `Children` and `GetZone()`. The `Model.Header` idiom of templates becomes `Shape.GetZone("Header")` or, in Razor, `@await RenderZoneAsync("Header")`, which renders nothing when the zone is absent.

### 4.2 `AlternateSet` and `ShapeKey`: alternates computed once, resolution cached

Alternates stay strings, with today's exact names, because that is what theme authors write as file names. What changes is their lifecycle:

- An `AlternateSet` is an immutable `string[]` computed by a convention (part type + part name + content type + display type, ...) and **cached by the convention's key**, as the `*AlternatesFactory` classes already do. A node references one or two sets; it never copies names.
- A `ShapeKey` is `(Type, AlternateSet, PlacementAlternates)`, compared by **reference** of the sets. Two `TitlePart` shapes of two `BlogPost` items in `Summary` have the same key.
- The `TemplateTable` (what a shape table becomes) caches `ShapeKey → RenderPlan`. The probing of alternates against the table runs once per key per tenant and theme, instead of once per shape. The cache is bounded by the number of (type, conventions) combinations a site has, in the low thousands at most.
- `Metadata.Alternates.Add(...)` from code at render time, today's `OnDisplaying` handlers, becomes an **alternates provider**: `builder.Describe("Menu").Alternates(node => MenuAlternates.For(node.Model))` returns an `AlternateSet` from a cache. Providers that need per-instance names (e.g. the term id) return a set with the same reference for the same input, so the resolution stays cached per distinct input, which is what the current code does implicitly through the string names anyway.
- The **per-content-item-id alternates** are removed from the defaults and offered by a theme opt-in (a shape table provider or a theme manifest flag). A theme that relies on `Content-42.cshtml` enables it; everyone else stops paying for it.

The `__` fallback of shape types (`Car__Toyota__Hybrid` → `Car__Toyota` → `Car`) and the alternates ordering (last added wins) are unchanged, so `placement.json` `alternates`, `wrappers` and `shape` entries keep working.

### 4.3 `RenderPlan` and `IShapeTemplate`: rendering that streams

```csharp
public interface IShapeTemplate
{
    ValueTask RenderAsync(ShapeRenderContext context);
}

public readonly struct ShapeRenderContext
{
    public ShapeNode Shape { get; }
    public TextWriter Output { get; }
    public T Model<T>();
    public ValueTask RenderAsync(ShapeNode child);        // in place
    public ValueTask RenderZoneAsync(string zone);        // nothing when absent
    public ValueTask RenderChildContentAsync();           // from a wrapper
}
```

- A `RenderPlan` is the resolved `IShapeTemplate`, its wrapper templates, and the per-type hooks that survive (see 4.6). It is immutable and shared.
- Templates **write to the output** instead of returning `IHtmlContent`. A parent template renders a child in place; a wrapper renders its child content in place. There is no buffer per shape and no copy per tree level. Razor already writes to a `TextWriter`; Fluid does too; code templates get the writer.
- **Caching becomes a pipeline step, not an event.** When a node has a `CacheContext`, the renderer looks the fragment up; on a hit it writes the cached HTML; on a miss it renders the subtree into a tee writer that both streams and captures. The Dynamic Cache module keeps its service, contexts and tags, and loses its three event handlers and the second serialization.
- `ShapeRenderContext` is a struct; rendering a shape allocates nothing beyond what its template allocates. The `Render shape` activity and the meter stay, behind the same `IsEnabled` checks.

### 4.4 Razor and Liquid templates

Theme authors keep writing `Views/Content-BlogPost.Summary.cshtml` or `.liquid`. What changes is how a template is executed and what `Model` is.

**Razor.** Shape templates are compiled by the Razor SDK as today, but derive from an Orchard `ShapeTemplatePage<TModel>` (a `RazorPage<TModel>` with the shape helpers) and are executed directly by the renderer: the compiled page is obtained from the `IRazorPageFactoryProvider` (cached per path), activated with the current `ViewContext` and the node's model, and its `ExecuteAsync()` writes into the current output. This skips `IHtmlHelper.PartialAsync()`, `ViewEngine.FindView()`, `RazorView` and the per-partial `ViewDataDictionary`, which is what the Liquid engine already does for its templates. Tag helpers, `@inject`, `T`, `Orchard`, `Url` and `Html` keep working, since the page is still a `RazorPage` in a `ViewContext`.

In a template:

```cshtml
@model TitlePartViewModel
<h1>@Model.Title</h1>
```

```cshtml
@* Content.Summary.cshtml: the model is the content item, the zones come from the node. *@
@model ContentItem
<article class="content-@Model.ContentType.HtmlClassify()">
    <header>@await RenderZoneAsync("Header")</header>
    @await RenderZoneAsync("Content")
    @if (HasZone("Footer"))
    {
        <footer>@await RenderZoneAsync("Footer")</footer>
    }
</article>
```

`Shape` (the `ShapeNode`), `RenderZoneAsync`, `HasZone`, `RenderAsync(ShapeNode)`, `RenderChildContentAsync` (in a wrapper) and `DisplayAsync` (kept as an alias of `RenderAsync` for shapes) are members of the page. A template without `@model` gets `object` as its model type, not `dynamic`; see 5.1 for the compatibility adapter.

**Liquid.** `Model` is the model object, exposed through Fluid's member access as today for typed models. The shape members move to a `Shape` value: `{{ Shape.Type }}`, `{% zone "Header" %}` renders a zone, `{% render_child_content %}` in a wrapper. The `shape_render`, `shape_new`, `shape_add_properties` and `{% shape %}` filters and tags keep working on nodes; `shape_add_properties` targets a dictionary model.

**Code.** `[Shape]` methods become `IShapeTemplate` implementations or methods taking `ShapeRenderContext`, registered by an `IShapeTemplateProvider`. The parameter binding by name (`Shape`, `DisplayAsync`, `New`, `Html`, property names) is replaced by the context, which removes the per-call reflection-built argument arrays.

### 4.5 Drivers and placement

Drivers keep their shape: `Display()`, `Edit()`, `UpdateAsync()`, `Initialize<TModel>()`, `Location()`, `Differentiator()`, `OnGroup()`, `RenderWhen()`, `Cache()`. What changes is underneath:

- `Initialize<TModel>()` creates **the model** (`new TModel()` + the initializer) and a `ShapeNode` around it. No Castle proxy, so `TModel` can be sealed, a record, or a struct boxed. `View(shapeType, model)` creates a node around an existing model. `Dynamic()` and `Copy()` are removed; `Factory()` creates a node.
- The `ContentPartDisplayDriver<T>` and `ContentFieldDisplayDriver<T>` base classes attach the cached `AlternateSet` of the part or field to the node when the result is applied, instead of registering an `OnDisplaying` handler per result.
- **Placement rules are compiled** once per shape table into `PlacementRule` records (display type, differentiator, content type, path matchers, location, alternates, wrappers) and resolved by a loop over the rules of the shape type, instead of a chain of closures built at discovery time. `placement.json` and the Placements module produce the same rules. The resolution result (rule, parsed location) can itself be cached per `(type, displayType, differentiator)` for rules without content type or path filters.
- `ShapeResult.ApplyAsync()` becomes: resolve placement → hidden? stop → group mismatch? stop → create the model and node → set alternates and wrappers from the rule → walk the zones (`GetOrAddZone`) → `Add(node, position)`. One node and one model allocated per placed shape.

### 4.6 Which extension points stay

| Today | Usage in the repository | Proposed |
| --- | ---: | --- |
| `OnDisplaying` on a type (add alternates from the shape's data) | 54 | An **alternates provider** on the type, returning a cached `AlternateSet` (4.2). |
| `OnProcessing` on a type (load data only when the fragment really renders, so the cache can skip it) | 14 | A **`Prepare` hook on the plan**, run after the cache lookup misses and before the template. Same purpose, same position, one delegate array per type. |
| `OnCreated` / `OnCreating` on a type (default values, base type) | 9 | Removed. Defaults belong to the model's constructor; the node is created by the renderer's factory, which is configurable per type if ever needed. |
| `OnDisplayed` on a type (post-process the HTML) | 4 | A **wrapper template**, which is what they are. |
| `ShapeMetadata.OnDisplaying/OnProcessing/OnDisplayed` on an instance | few, mostly through `ShapeResult.Displaying()` | `ShapeResult.Displaying()` is kept for alternates and metadata tweaks at placement time; the other instance events are removed. |
| `IShapeDisplayEvents` (global) | Dynamic Cache, MiniProfiler | Caching is a pipeline step (4.3); profiling uses the `Render shape` activity. The interface is removed. |
| `IShapeFactoryEvents` (global) | none | Removed. |
| `IShapeBindingResolver` (Templates module: templates stored in the database) | 2 | Becomes a **template source** of the `TemplateTable`: it contributes named templates when the table is built or when a template is saved, and invalidates the plan cache. It is no longer asked per shape and per alternate. |
| `IShapeTableProvider` / `ShapeTableBuilder.Describe()` | 28 providers, 70 `Describe` | Kept, with `BoundAs(IShapeTemplate)`, `Alternates(...)`, `Prepare(...)`, `Wrappers(...)`, `Placement(...)`. |
| `IShapePlacementProvider` | Placements module, content types editor, workflows | Kept, produces `PlacementRule`s. |
| `DisplayAsAsync(shape, type)` (morph) | 3 templates | `RenderAsync(node with { Type = ... })`: a node is cheap to copy. |
| `<shape>`, `<metadata>`, `<add-property>` tag helpers, `{% shape %}` | 13 + 15 | Kept, they create nodes with a dictionary model. |
| `IShapeHtmlRenderer` (render to a string outside a view) | API endpoints | Kept, renders a node to a `StringWriter`. |
| Shape debug comments, `Render shape` activity, meter | — | Kept. |

### 4.7 The shape table

- Harvesting is unchanged in principle (templates by file name, `[Shape]` methods, `Describe()` providers), but produces a `TemplateTable` of `name → IShapeTemplate` plus per-type plans data, with no `Func<DisplayContext, Task<IHtmlContent>>` indirection.
- The file enumeration result is cached per extension across tenants, since a module's files don't depend on the tenant; only the composition per theme is per tenant.
- Templates stored in the database (Templates module) are a table source that can be invalidated.

## 5. Compatibility and breaking changes

The proposal is a major-version change of the display management API. The surface that breaks, and how much of it is ours to fix:

| Change | Breaks | Repository impact | Mitigation |
| --- | --- | ---: | --- |
| `IShape`, `Shape`, `Composite`, `ShapeViewModel`, `ZoneHolding`, `ZoneOnDemand`, `Nil` are replaced by `ShapeNode` + a model. | Code that implements `IShape`, casts to `Shape`, or reads `Properties` / `Items` / `Named()`. | 57 module files mention `IShape`; 107 `Properties["..."]`, 124 `GetProperty<T>` call sites, 22 templates read `Model.Items`. | Mechanical: `Properties` reads become model properties or dictionary reads; `Items` becomes `Shape.Children`. |
| `dynamic` is gone from shapes: `New.Foo()`, `Model.Foo` without `@model`, `dynamic` zones. | Templates without `@model` (688 in the repository) that read ad hoc properties or zones; 74 `dynamic` usages in module code; 9 `New.` in code, 150 `New.` in templates (`Hint_Admin`, `DateTime`, `Timespan`, ...). | Large but shallow: most untyped templates read 2 to 5 members. | A **compatibility adapter** (5.1) keeps untyped templates working during the transition; an analyzer flags `dynamic` shape usage. |
| Typed shapes are models, not proxies. | Code that casts a shape to its view model *and* to `IShape`. | `Initialize<T>` call sites (453) don't change; the few `(IShape)model` casts in templates do. | The template gets both `Model` and `Shape`. |
| Lifecycle events reduced (4.6). | Providers using `OnCreated`, `OnDisplayed`, `IShapeDisplayEvents`, `IShapeFactoryEvents`. | 9 + 4 + 2 usages. | Each has a direct replacement. |
| `IShapeBindingResolver` becomes a template source. | The Templates module and any custom resolver. | 2. | Ours. |
| `Metadata.ChildContent` is gone; wrappers use `RenderChildContentAsync()`. | Wrapper templates. | 8 templates. | Mechanical. |
| Per-content-item-id alternates are opt-in. | Themes with `Content-42.cshtml` files. | None in the repository. | A theme setting. |
| `IHtmlContent`-returning `DisplayAsync()` becomes write-in-place. | Code that captures the HTML of a shape. | `IShapeHtmlRenderer`, Dynamic Cache, a few templates assigning `var x = await DisplayAsync(...)`. | `RenderToStringAsync()` helper; `DisplayAsync` in a template still works because it writes to the page output. |

What does **not** break: template file names and folders, alternates names, `placement.json`, the Placements module data, display types, `Location()` strings, grouping (`#Tab`, `%Card`, `|Column`), the `Templates` module's named templates, the `<shape>` tag helper and Liquid tags, `IDisplayManager<T>` / `IContentItemDisplayManager` entry points, admin lists (`AdminListContext`, column providers).

### 5.1 Compatibility adapter for untyped templates

A Razor shape template without `@model` compiles against `object`. To keep existing themes rendering during the transition, the renderer can hand such templates a `DynamicShapeAdapter : DynamicObject` that resolves `Model.X` to: the model's property `X` (reflection, cached per type), then the zone `X`, then the child named `X`, then `Nil`. It is an opt-in cost paid only by templates that haven't been migrated, it lives in a separate compatibility package, and it is removed one major version later. The core templates are migrated in the same change.

### 5.2 Migration tooling

- An analyzer in `OrchardCore.Localization.Analyzers`'s sibling project flags `dynamic` shape usage, `New.`, `Properties[...]` on shapes, and templates without `@model` in theme projects.
- The shape debug comments (`<!--shape-start type:... bindings:...-->`) already show which binding renders a fragment; adding the model type to them helps migrating templates.
- The release notes list the mechanical rewrites with before/after samples, as in section 4.4.

## 6. Expected gains

`ShapePrototypeBenchmark` runs the scenarios of `ShapePipelineBenchmark` on the prototype, with the same markup written by code templates:

| Scenario | Current pipeline | Prototype | Time | Memory |
| --- | ---: | ---: | ---: | ---: |
| Create a typed shape | 201 ns, 584 B | 30 ns, 144 B | ÷ 6.7 | ÷ 4 |
| Render leaf: no alternates | 1,215 ns, 928 B | 84 ns, 144 B | ÷ 14 | ÷ 6.4 |
| Render leaf: 5 alternates, none bound | 1,891 ns, 1,440 B | 81 ns, 144 B | ÷ 23 | ÷ 10 |
| Render leaf: 5 alternates + 1 wrapper | 2,358 ns, 2,072 B | 303 ns, 336 B | ÷ 7.8 | ÷ 6.2 |
| Place one driver result | 1,164 ns, 688 B | 133 ns, 560 B | ÷ 8.8 | ÷ 1.2 |
| Content tree: build 6 results, render 9 shapes | 22,541 ns, 16,160 B | 1,031 ns, 1,576 B | ÷ 22 | ÷ 10 |

In the prototype the 144 bytes of a leaf are the node and its model; rendering it allocates nothing, and alternates cost nothing once their key is cached. The placement row allocates the zone node and its children list, which the current pipeline shares across the benchmark; in the tree row, where both build the same structure, the prototype allocates ten times less.

The prototype is not a complete implementation (no Razor compilation, no cache step, no editor grouping), but it exercises the hot path: node creation, placement with rules, key-cached resolution, wrappers and streaming into a writer. The gains that are structural, and don't depend on the quality of the prototype:

- Per shape: no `DisplayContext`/`ShapeDisplayContext`/`ShapeCreatingContext`/`ShapeCreatedContext`, no alternates copy, no probing, no proxy, no `IHtmlContent` buffer and copy. On the content tree scenario that is 20 times less CPU and 10 times less memory in the pipeline itself.
- Per page: the Razor partial overhead per shape is removed by executing pages directly, as the Liquid engine already does. {{RAZOR_GAIN}}
- Memory: a shape is one node (≈ 100 bytes) and its model, instead of a `Shape` (properties dictionary, metadata, alternates dictionary, classes list, attributes dictionary, items list, proxy, mixin).
- Startup: no proxy type generation; file enumeration shared across tenants.

What the redesign does not change: the time spent *inside* templates (Razor code, tag helpers, localization, URL generation, content queries). On a real page that is a large share of the total, and it is where profiling a slow page should go next, with the `Render shape` activities.

## 7. Alternatives considered

**A. Keep the model, optimize the implementation.** Cache the resolution by alternates set, stop allocating `ZoneOnDemand` on reads, remove the proxies for models implementing `IShape`, flatten the placement closures, parse positions once. This is Phase 1 below, and it is worth doing regardless. It doesn't remove the DLR, the per-shape contexts, the `IHtmlContent` copies, the events, or the one-object-for-everything coupling, and every future optimization keeps fighting the dynamic surface.

**B. Razor Components (Blazor static SSR) as the fragment model.** Components are typed, fast, and compose with `RenderFragment`. But the override-by-file-name convention, Liquid themes, `placement.json`, the Templates module and the tag helpers would all have to be rebuilt on top of component types, with a type resolver per theme that is a shape table in disguise. Themes written as Razor components are not something the Orchard Core community has asked for, and Blazor SSR has no tag helpers, no `@inject` of scoped MVC services in the same way, and a different `HttpContext` story. Not pursued as the model; the admin UI could use components where it is an app rather than a theme, independently of this proposal.

**C. No tree: view components and tag helpers only.** Each driver returns HTML directly. It loses placement, zones, display types as data, and the ability for a theme to reorder what a module emits. It is the capability list of section 1 without composition.

**D. Keep `dynamic`, cache harder.** The DLR cost is small per access; the real costs are elsewhere. But `dynamic` is what prevents AOT, hides errors until runtime, and is the reason the model and the node are the same object. Removing it is what makes the rest simple.

## 8. Plan

**Phase 0 (this branch): measure.** The benchmarks and the prototype, so that every later change is compared against the same numbers.

**Phase 1 (non-breaking, 4.x): the structural fixes under the current API.**

1. Resolution cache in `DefaultHtmlDisplay` keyed by `(ShapeTable, type, alternates)`, with the alternates factories exposing their cached arrays as immutable sets the shape references rather than copies; per-id content alternates behind a setting.
2. `ZoneHolding.TryGetMember` stops allocating and storing a `ZoneOnDemand` for a read; `Nil` is returned and `AddAsync` on it creates the zone.
3. `Initialize<TModel>` without Castle when `TModel` implements `IShape` or when a `ShapeViewModel<TModel>` wrapper is acceptable to the template (`@model` of the wrapper).
4. Placement predicates flattened; `PlacementInfo` results cached per rule and key.
5. `IShapeBindingResolver` consulted once per shape type per request (the Templates module already caches `_isAdmin`; the resolver can be asked for the whole alternates list at once).
6. Razor shape templates executed through the page factory instead of `PartialAsync`.
7. Positions parsed at `AddAsync` time.

Each item is a self-contained PR with its benchmark.

**Phase 2 (breaking, 5.0): the new model.** `ShapeNode`, `AlternateSet`/`ShapeKey`, `TemplateTable`/`RenderPlan`, `IShapeTemplate`, streaming rendering, `ShapeTemplatePage<TModel>`, the Liquid `Shape` value, drivers on nodes, the compatibility adapter, core templates migrated, analyzer, docs and release notes.

**Phase 3 (5.x → 6.0):** remove the compatibility adapter and the `dynamic` package.

## 9. Open questions

1. **Should `ShapeNode` be public and mutable, or should templates see a read-only view?** Mutability is what makes `OnDisplaying`-style tweaks and the `<metadata>` tag helper simple; a read-only interface for templates would prevent a template from re-placing its siblings.
2. **Editors' grouping (tabs, cards, columns)** is implemented by the `ContentZone` code shape re-grouping its children at render time. With nodes, grouping can be done once when the editor tree is built, which also makes the group containers plain nodes. To confirm with the `ocat-*` admin edit views.
3. **Liquid access to the model.** Fluid's member access on arbitrary CLR models is already allowed for shapes through `ShapeAccessor`; with plain models, the allow-list of accessible types needs a decision (register view model types, or allow any model handed to a template).
4. **`IShapeHtmlRenderer` and the Dynamic Cache** both need a captured string; the tee writer must handle nested cached shapes (today's stack of cache contexts), which the prototype doesn't cover.
5. **The name.** Keeping "shape" for the node keeps the documentation and the community vocabulary; the alternative, "fragment", describes it better but renames twenty years of material.
