# Scripting `OrchardCore.Scripting`

## Purpose

The scripting module provides an API allowing you to evaluate custom scripts in different languages.

## Usage

### Executing some script

The main interface is `IScriptingManager`.

```csharp
public interface IScriptingManager
{
    IScriptingEngine GetScriptingEngine(string prefix);
    object Evaluate(string directive);  
    IList<IGlobalMethodProvider> GlobalMethodProviders { get; }
}
```

To evaluate an expression using a scripting engine, you must know which ones are available in the system.  
For instance, a JavaScript one is available by default and its prefix is `js`.  
To return the current date and time as a string we could do something like this:

```csharp
var scriptingManager = serviceProvider.GetService<IScriptingManager>();
var date = scriptingManager.Evaluate("js: Date().toISOString()");
```

The `js:` prefix is used to describe in which language the code is written. Any module can provide
a new scripting engine by implementing the `IScriptingEngine` interface.

### Customizing the scripting environment

Any module can provide custom methods for scripts independently of the chosen language.  
For instance the `Contents` module provides a `uuid()` helper method that computes a unique content item identifier.

To create a global method, implement `IGlobalMethodProvider` then add it to the current `IScriptingManager` instance like this:

```csharp
var scriptingManager = serviceProvider.GetService<IScriptingManager>();
var globalMethodProvider = new MyGlobalMethodProvider();
scriptingManager.GlobalMethodProviders.Add(globalMethodProvider);
```
Or register your GlobalMethodProvider as a singleton in your Module's `Startup`
```csharp
 services.AddSingleton<IGlobalMethodProvider, MyGlobalMethodProvider>();
```

## File

The File scripting engine provides methods to read file contents.

| Name | Example | Description |
| ---- | ---- | -------- |
| `text` | `file:text('../wwwroot/template.html')` | Returns the content of a text file. |
| `base64` | `file:base64('../wwwroot/image.jpg')` | Returns the base64 encoded content of a file. |


## JavaScript `OrchardCore.Scripting.JavaScript`

The JavaScript scripting module implements a `IScriptingEngine` that uses [Esprima.NET](https://github.com/sebastienros/esprima-dotnet) to evaluate scripts.

### Methods

Here is a list of javascript methods provided by Orchard Modules.

#### Content (`OrchardCore.Contents`)
| Function | Description 
| -------- | ----------- |
|`newContentItem(contentTypeName: String): IContent`| Creates a new instance of a ContentType (does not persist)|
|`createContentItem(contentTypeName: String, publish: Boolean, properties: Object): IContent`| Creates and persists a new ContentItem. Conditionally publishes it. |
|`updateContentItem(contentItem: IContent, properties: Object)`| Updates an existing content item with the properties |
|`getUrlPrefix(): `||
#### HTTP (`OrchardCore.Workflows.Http`)

| Function | Description 
| -------- | ----------- |
|  `httpContext(): HttpContext` | Returns the `HttpContext` which encapsulates all HTTP-specific information about an individual HTTP request. |
| `queryString(name: String): String | Array` | Returns the entire query string (including the leading `?`) when invoked with no arguments, or the value(s) of the parameter name passed in as an argument. |
| `responseWrite(text: String): void` | Writes the argument string directly to the HTTP response stream. |
| `absoluteUrl(relativePath: String): String` | Returns the absolute URL for the relative path argument.  |
| `readBody(): String` | Returns the raw HTTP request body.  |
| `requestForm(name: String): String | Array` | Returns the value(s) of the form field name passed in as an argument. |
| `queryStringAsJson(): JObject` | Returns the entire query string as a JSON object. Example: ` { "param1": [ "param1-value1", "param1-value2" ], "param2": [ "param2-value1", "param2-value2" ], ... }` |
| `requestFormAsJson(): JObject` | Returns the entire request form as a JSON object. Example: ` { "field1": [ "field1-value1", "field1-value2" ], "field2": [ "field2-value1", "field2-value2" ], ... }` |


