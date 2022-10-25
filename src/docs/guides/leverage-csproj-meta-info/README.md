# Leveraging your CSPROJ meta information

## Motivation

Out of the box, a `Manifest.cs` file is generated, which includes the `ModuleAttribute`, `FeatureAttribute`, `ThemeAttribute`, and so forth, that describe the _Orchard Core_ meta information about your projects. However, let's say you want to connect available meta properties at the `CSPROJ` level, i.e. such properties as `$(MSBuildProjectName)`, `$(AssemblyVersion)`, `$(PackageTags)`, to name a few.

At its core, we wanted to follow a pattern similar to `InternalsVisibleTo`, i.e.

```xml
<ItemGroup>
    <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleTo">
      <_Parameter1>Test.$(MSBuildProjectName)</_Parameter1>
    </AssemblyAttribute>
</ItemGroup>
```

The principles are the same, but in prior versions, the attribute constructors were not available for us to lean into. We also decided to provide helpful [`MSBuild` Items](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-items) as a more accessible, self-documenting shorthand input. So, while it is possible to engage directly with the constructors, i.e. `InternalsVisibleTo`, we will leave that as an exercise for the reader. Rather, we will focus on the [_Item Lists_](#msbuild-item-lists).

We also had a requirement that our assembly versions, in particular, are bumped during the build targets, so we wanted that to happen before being relayed into the _Orchard Core_ targets. Consult with the [Microsoft `MSBuild` documentation](https://docs.microsoft.com/en-us/visualstudio/msbuild) for broader details concerning the build properties that are available.

## MSBuild Item Lists

There are several items you may be interested in, depending on your projects, i.e. whether _Module_, _Theme_, and/or any _Features_. There are also a couple of properties that we use during the _Orchard Core_ targets. Obviously, one would not specify all of these in the same project, but for documentation brevity, are enumerated here. There are [some caveats](#some-caveats) to mention, but overall, these are the attributes of interest.

```xml
<OrchardCoreFeatures Include="..."
                     Name="..."
                     Category="..."
                     Priority="..."
                     Description="..."
                     Dependencies="..."
                     DefaultTenant="..."
                     AlwaysEnabled="..."
                     EnabledByDependencyOnly="..." />
```

```xml
<OrchardCoreModules Include="..."
                    Name="..."
                    ModuleType="..."
                    Category="..."
                    Priority="..."
                    Description="..."
                    Author="..."
                    Version="..."
                    Website="..."
                    Dependencies="..."
                    Tags="..."
                    DefaultTenant="..."
                    AlwaysEnabled="..."
                    EnabledByDependencyOnly="..." />
```

```xml
<OrchardCoreThemes Include="..."
                   Name="..."
                   Base="..."
                   Category="..."
                   Priority="..."
                   Description="..."
                   Author="..."
                   Version="..."
                   Website="..."
                   Dependencies="..."
                   Tags="..."
                   DefaultTenant="..."
                   AlwaysEnabled="..."
                   EnabledByDependencyOnly="..."/>
```

With all _properties_ described as follows.

|Property|Type|Description|
|-|-|-|
|`Include`|`string` <sup>1</sup>|Required, signals `MSBuild` that an _item list_ is supported, which `Identity` serves as the attribute `Id`.|
|`Name`|`string`|Optional, human readable name for the module or feature.|
|`Base`|`string`|Optional, the base theme from which the assembly is derived. Not applicable to either _features_ or _modules_.|
|`ModuleType`|`string`|Optional author provided `Type`; defaults to `"Module"` or `"Theme"`. Not applicable to _features_ since this is a _Module_ base class property. <sup>2</sup>|
|`Category`|`string`|Optional category for the feature.|
|`Priority`|`int` <sup>1</sup>|Optional integer priority, given as a `string`, defaults to `0`; lower priority given precedence.|
|`Description`|`string`|Optional descriptive text.|
|`Author`|`string`|Optional, author provided identification.|
|`Version`|`string`|Optional, recommended, semantic versioning text, defaults to `"0.0"`.|
|`Dependencies`|`list`|Optional, semi-colon delimited list of _Module Identifier_ dependencies. <sup>3</sup>|
|`Tags`|`list`|Optional, semi-colon delimited list of tags. <sup>3</sup>|
|`DefaultTenant`|`bool` <sup>1</sup>|Optional, Boolean, `true|false`, defaults to `false`.|
|`AlwaysEnabled`|`bool` <sup>1</sup>|Optional, Boolean, `true|false`, defaults to `false`.|
|`EnabledByDependencyOnly`|`bool` <sup>1</sup>|Optional, Boolean, `true|false`, defaults to `false`.|

<sup>[1] `MSBuild` relays all meta data as `string`, leaving authors to contend with either `string` or `object` type conversions i.e. either `int` or `bool`, which is fine for our purposes.</sup>
<br/><sup>[2] Depending on the `Attribute` context, `ModuleAttribute` yields `"Module"`, `ThemeAttribute` yields `"Theme"` by default.</sup>
<br/><sup>[3] Defaults to _semi-colon_ delimited, since that is also the `CSPROJ` delimiter. Also supports _space_ or _comma_ delimiting.</sup>

And for convenience, you may specify all of your assembly Orchard Core attributes using one, top level item list.

```xml
<OrchardCoreAttributes Include="..."
                       Type="..."
                       ... />
```

|Property|Type|Description|
|-|-|-|
|`Type`|`string`|Commonly either, `"feature"`, `"theme"` or `"module"`. Anything else not `"feature"` or `"theme"` assumes _Module_.|

Otherwise, the `OrchardCoreAttributes` should include all the same _properties_ that the desired `Type` would require.

## Targets order dependencies

A couple of convenience [`MSBuild` _properties_](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-properties) are provided, which authors may leverage in order to accommodate their internal target build orders.

|Property|Default|Description|
|-|-|-|
|`OrchardCoreEmbeddingAfterTargets`|`AfterResolveReferences`|_Orchard Core_ assembly embedding to occur **_after_** these targets.|
|`OrchardCoreEmbeddingBeforeTargets`|`GenerateAssemblyInfo`|_Orchard Core_ assembly embedding to occur **_before_** these targets.|

We do not recommend changing the defaults in any way, but rather to append with your custom targets as desired, i.e.

```xml
<PropertyGroup>
  <OrchardCoreEmbeddingAfterTargets>$(OrchardCoreEmbeddingAfterTargets);MyCustomTarget<OrchardCoreEmbeddingAfterTargets>
  <!--                                                                 ^^^^^^^^^^^^^^^ -->
</PropertyGroup>
```

### Some caveats

Per assembly, the following heuristics are detected by the _Orchard Core_ build targets:

* Only one _Module_ may be defined
* Only one _Theme_, which is also a _Module_ may be defined
* A _Module_ and a _Theme_ may not both be defined
* Any number of purely _Feature_ (i.e. non-_Module_ nor _Theme_) attributes may be defined

## Drop the Manifest CSharp file

When you are happy with your `CSPROJ` item lists, then you may comment out the contents of the `Manifest.cs` file, or delete it altogether from your project, at your discretion.

## Summary

In this article we described how to leverage _Orcard Core_ `CSPROJ` _item lists_ and _properties_ in order to enrich and get the absolute most from your authoring experience.

Happy coding!
