# Scripting (`OrchardCore.Scripting`)

## Purpose

The scripting module provides an API allowing you to evaluate custom scripts in different languages.

## Usage

### Executing some script

The main interface is [IScriptingManager](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore/OrchardCore.Infrastructure.Abstractions/Scripting/IScriptingManager.cs)

To evaluate an expression using a scripting engine, you must know which ones are available in the system.  
For instance, a JavaScript one is available by default and its prefix is `js`.  
To return the current date and time as an object we could do something like this:

```csharp
var scriptingManager = serviceProvider.GetService<IScriptingManager>();

// Find the javascript engine by its prefix
var engine = scriptingManager.GetScriptingEngine("js");

// Find all global methods in the system. Here you could add more methods to the scope as needed
var globalMethods = _scriptingManager.GlobalMethodProviders.SelectMany(x => x.GetMethods());

// Create scope for the engine
var scope = engine.CreateScope(globalMethods, serviceProvider, null, null);

// Evaluate the given script
var date = engine.Evaluate("js: new Date().toISOString()");
```

The `js:` prefix is used to describe in which language the code is written. Any module can provide
a new scripting engine by implementing the `IScriptingEngine` interface.

### Customizing the scripting environment

Any module can provide custom methods for scripts independently of the chosen language.  
For instance the `Contents` module provides a `uuid()` helper method that computes a unique content item identifier.

To create a global method, implement the `IGlobalMethodProvider`. Then, add it to the current `IScriptingManager` instance by registering it as a singleton in your Module's `Startup`

```csharp
 services.AddSingleton<IGlobalMethodProvider, MyGlobalMethodProvider>();
```

## File

The File scripting engine provides methods to read file contents.

| Name     | Example                                 | Description                                   |
|----------|-----------------------------------------|-----------------------------------------------|
| `text`   | `file:text('../wwwroot/template.html')` | Returns the content of a text file.           |
| `base64` | `file:base64('../wwwroot/image.jpg')`   | Returns the base64 encoded content of a file. |

## JavaScript `OrchardCore.Scripting.JavaScript`

The JavaScript scripting module implements an `IScriptingEngine` that uses [Jint](https://github.com/sebastienros/jint) to evaluate scripts.

### Configuring the JavaScript engine

The engine is configured through Jint's own options type, which is registered as `IOptions<Jint.Options>`:

```csharp
services.Configure<Jint.Options>(options =>
{
    options.MaxStatements(10_000);
    options.TimeoutInterval(TimeSpan.FromSeconds(5));
});
```

Three things are worth knowing about how that instance is used:

- **One `Jint.Options` instance is shared by every engine of the tenant.** Configure only settings that make sense for the whole tenant, since the same options serve recipe execution, workflow scripts and layer rules alike. Use the constraint helpers shown above (`MaxStatements`, `TimeoutInterval`, `LimitMemory`, `CancellationToken`): each of them registers a *factory*, so every engine gets its own counter and its own deadline and the shared instance stays safe for concurrent requests. Registering a constraint *instance* — `options.Constraint(new MyConstraint())` — shares that instance, and with it its per-execution state, across every concurrently running engine of the tenant. Derive from `Jint.Constraint` and register it with the factory overload, `options.Constraint(() => new MyConstraint())`, instead.
- **A limit spelled as a saturated or absent value registers nothing.** `MaxStatements(int.MaxValue)`, `LimitMemory(long.MaxValue)` and `TimeoutInterval(TimeSpan.MaxValue)` produce exactly the same engine as never calling the method, and additionally remove any limit of that kind set earlier. The same is true of `MaxStatements()` with no argument: its parameter defaults to `0`, and only a positive budget registers a constraint, so that call reads like it turns a statement limit on while leaving the statement count unlimited. Always pass the budget you mean, and omit the call rather than passing a maximum value.
- **Do not register an `IObjectConverter` that handles `Delegate`.** Global methods are `Delegate` values, and converters are consulted before Jint's own delegate wrapping, so such a converter would change the shape of the globals that are created on demand while leaving the eagerly created ones alone.

No execution constraints are configured by default, so a script such as `while (true) {}` runs until the process is recycled. Sites that let non-administrators author scripts should set at least one.

### Global methods are created on demand

The globals contributed by `IGlobalMethodProvider` implementations are declared on every engine but are not built until a script reads the name. A recipe expression such as `[js:uuid()]` therefore only pays for `uuid`, not for every registered global. This is not observable from script — the properties exist, they are non-enumerable, and the value a name resolves to is stable for the whole evaluation — but the `Func<IServiceProvider, Delegate>` you supply is invoked lazily, and not at all when the script does not use the method. Keep it free of side effects that the surrounding code depends on.

Three kinds of method are created eagerly instead, so a factory backing one of them runs once per engine whether or not the script uses it:

- Methods passed directly to `IScriptingEngine.CreateScope()` rather than registered through DI, such as a recipe's `variables()` or a workflow's `workflow()`. These also take precedence over a registered global of the same name.
- A name contributed by more than one registered provider, because which provider wins depends on the order the methods are set in.
- A method carrying an asynchronous variant whose `<name>Async` global is also claimed by a method literally named `<name>Async`.

### Methods

Here is a list of javascript methods provided by Orchard Modules.

#### Generic functions

| Function                                                | Description                                                                                                                     |
|---------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------|
| `log(level: String, text: String, param: Object): void` | Formats and writes a log message at the specified log level.                                                                    |
| `uuid(): String`                                        | Generates a unique identifier for a content item.                                                                               |
| `base64(String): String`                                | Decodes the specified string from Base64 encoding. Use <https://www.base64-image.de/> to convert your files to base64.          |
| `html(String): String`                                  | Decodes the specified string from HTML encoding.                                                                                |
| `gzip(String): String`                                  | Decodes the specified string from gzip/base64 encoding. Use <http://www.txtwizard.net/compression> to gzip your strings.        |
| `protect(purpose: String, value: String): String`       | Protects the specified value using the ASP.NET Core Data Protection API with the given purpose string.                          |
| `encrypt(value: String): String`                        | Encrypts the specified value using the ASP.NET Core Data Protection API. Returns a Base64-encoded ciphertext.                   |
| `decrypt(value: String): String`                        | Decrypts a Base64-encoded string previously encrypted with the `encrypt` function. Returns an empty string if decryption fails. |

!!! warning
    The `protect` function is intended for use during development and testing scenarios only. **Storing secrets in recipe files for production environments is not recommended** and should be avoided. Use a secure secret management solution (e.g., Azure Key Vault, environment variables) for production deployments.

**Example (protect):**

```json
{
  "steps": [
    {
      "name": "settings",
      "Properties": {
        "ApiKey": "[js: protect('MyModule.ApiKey', 'my-secret-value')]"
      }
    }
  ]
}
```

**Example (encrypt / decrypt):**

```javascript
var encryptedValue = encrypt('my-secret-value');
```

To read the value back later using JavaScript:

```javascript
var plainText = decrypt(encryptedValue);
```

#### Content (`OrchardCore.Contents`)

| Function                                                                                                   | Description                                                         |
|------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------|
| `newContentItemAsync(contentTypeName: String): Promise<IContent>`                                          | Creates a new instance of a ContentType (does not persist)          |
| `createContentItemAsync(contentTypeName: String, publish: Boolean, properties: Object): Promise<IContent>` | Creates and persists a new ContentItem. Conditionally publishes it. |
| `updateContentItemAsync(contentItem: IContent, properties: Object): Promise`                               | Updates an existing content item with the properties                |
| `deleteContentItemAsync(contentItem: IContent): Promise`                                                   | Deletes an existing content item                                    |
| `getUrlPrefix(path: String): String`                                                                       | Prefixes the path with the Tenant prefix (if specified)             |

#### Layers (`OrchardCore.Layers`)

| Function                         | Description                                                                                                                          |
|----------------------------------|--------------------------------------------------------------------------------------------------------------------------------------|
| `isHomepage(): Boolean`          | Returns true if the current request Url is the current homepage                                                                      |
| `isAnonymous(): Boolean`         | Returns true if there is no authenticated user on the current request                                                                |
| `isAuthenticated(): Boolean`     | Returns true if there is an authenticated user on the current request                                                                |
| `url(url: String): Boolean`      | Returns true if the current URL matches the provided URL. Add a `*` to the end of the URL parameter to match any URL that starts with the provided value. |
| `culture(name: String): Boolean` | Returns true if the current culture name or the current culture's parent name matches the `name` argument                            |

#### Queries (`OrchardCore.Queries`)

| Function                                                                                               | Description                      |
|--------------------------------------------------------------------------------------------------------|----------------------------------|
| `executeQueryAsync(name: String, parameters: Dictionary<string,object>): Promise<IEnumerable<object>>` | Returns the result of the query. |

#### HTTP (`OrchardCore.Workflows.Http`)

| Function                                                             | Description                                                                                                                                                                 |
|----------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `httpContext(): HttpContext`                                         | Returns the `HttpContext` which encapsulates all HTTP-specific information about an individual HTTP request.                                                                |
| `queryString(name: String): String`                                  | Array`                                                                                                                                                                      | Returns the entire query string (including the leading `?`) when invoked with no arguments, or the value(s) of the parameter name passed in as an argument. |
| `responseWriteAsync(text: String): Promise`                          | Writes the argument string directly to the HTTP response stream.                                                                                                            |
| `absoluteUrl(relativePath: String): String`                          | Returns the absolute URL for the relative path argument.                                                                                                                    |
| `readBodyAsync(): Promise<String>`                                   | Returns the raw HTTP request body.                                                                                                                                          |
| `requestForm(name: String): String`                                  | Array`                                                                                                                                                                      | Returns the value(s) of the form field name passed in as an argument. |
| `deserializeRequestDataAsync(): Promise<Dictionary<string, object>>` | Deserializes the request data as a Dictionary<string, object> for requests that send JSON or form data. Replaces deprecated queryStringAsJson and requestFormAsJson methods |

#### Recipes (`OrchardCore.Recipes`)

| Function                                                   | Description                                                                                                                                                                                                            |
|------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `variables(): string`                                      | Declare variables at the root of a recipe. Ex: `"variables": { "blogContentItemId": "[js:uuid()]" }`  Retrieve a variable value like this: `"ContentItemId": "[js: variables('blogContentItemId')]"`                   |
| `parameters(): string`                                     | Retrieves the parameters specified during the setup. Ex: `"Owner": "[js: parameters('AdminUserId')]"` See the available [Setup Recipe parameters](../Setup/README.md#recipe-parameters)                                |
| `configuration(key: String, defaultValue: String): string` | Retrieves the specified configuration setting by its key, optionally providing a default. Ex: `[js: configuration('OrchardCore_Admin:AdminUrlPrefix', 'Admin')]` See [IShellConfiguration](../Configuration/README.md) |

#### Workflows (`OrchardCore.Workflows.Http`)

The following JavaScript functions are available by default to any workflow activity that supports script expressions:

| Function                                                                                          | Description                                                                                                                                                                                                                                                                                      |
|---------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `workflow(): WorkflowExecutionContext`                                                            | Returns the `WorkflowExecutionContext` which provides access to all information related to the current workflow execution context.                                                                                                                                                               |
| `workflowId(): String`                                                                            | Returns the unique workflow ID.                                                                                                                                                                                                                                                                  |
| `input(name: String): Any`                                                                        | Returns the input parameter with the specified name. Input to the workflow is provided when the workflow is executed by the workflow manager.                                                                                                                                                    |
| `output(name: String, value: Any): void`                                                          | Sets an output parameter with the specified name. Workflow output can be collected by the invoker of the workflow.                                                                                                                                                                               |
| `property(name: String): Any`                                                                     | Returns the property value with the specified name. Properties are a dictionary that workflow activities can read and write information from and to.                                                                                                                                             |
| `lastResult(): Any`                                                                               | Returns the value that the previous activity provided, if any.                                                                                                                                                                                                                                   |
| `correlationId(): String`                                                                         | Returns the correlation value of the workflow instance.                                                                                                                                                                                                                                          |
| `setCorrelationId(id:string): void`                                                               | Set the correlation value of the workflow instance.                                                                                                                                                                                                                                              |
| `signalUrl(signal: String): String`                                                               | Returns workflow trigger URL with a protected SAS token into which the specified signal name is encoded. Use this to generate URLs that can be shared with trusted parties to trigger the current workflow if it is blocked on the Signal activity that is configured with the same signal name. |
| `setOutcome(outcome: String): void`                                                               | Adds the provided outcome to the list of outcomes of the current activity                                                                                                                                                                                                                        |
| `createWorkflowToken(workflowTypeId: String, activityId: String, expiresInDays: Integer): String` | Generates a workflow SAS token for the specified workflowTypeid, activityId. You can also set the expiration date in number of days.                                                                                                                                                             |
