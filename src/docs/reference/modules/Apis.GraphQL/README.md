# GraphQL (OrchardCore.GraphQL)

## GraphQL

The GraphQL module allows client applications to query the content handled by an Orchard website.  
It enables the GraphiQL Explorer view to test GraphQL queries, and provides HTTP endpoints to send client queries.

## HTTP Methods, Headers, and Body

### GET request

When receiving an HTTP GET request, the GraphQL query should be specified in the "query" query string. For example, if we wanted to execute the following GraphQL query:

```graphql
{
  me {
    name
  }
}
```

This request could be sent via an HTTP GET like so:

`https://localhost:44300/api/graphql?query={me{name}}`

Query variables can be sent as a JSON-encoded string in an additional query parameter called variables. If the query contains several named operations, an operationName query parameter can be used to control which one should be executed.

### POST request

#### application/json content type

A standard GraphQL POST request should use the `application/json` content-type header, and include a JSON-encoded body of the following form:

```graphql
{
  "query": "...",
  "operationName": "...",
  "variables": { "myVariable": "someValue", ... }
}
```

`operationName` and `variables` are optional fields. `operationName` is only required if multiple operations are present in the query.

#### application/graphql content type

Another option is to use the `application/graphql` content-type header, and the HTTP POST body contents is treated as the GraphQL query string.

#### query string

In addition to the above, If the "query" query string parameter is present (as in the GET example above), it will be parsed and handled in the same way as the HTTP GET case.

### Response

Regardless of the method by which the query and variables were sent, the response is returned in the body of the request in JSON format.  
A query might result in some data and some errors, and those are returned in a JSON object of the form:

```json
{
  "data": { ... },
  "errors": [ ... ]
}
```

If there were no errors returned, the "errors" field is not present on the response.
If no data is returned the "data" field is only included if the error occurred during execution.

## Authentication

Executing a GraphQL query requires the issuer to have the `ExecuteGraphQL` permission. Like any other API in Orchard Core, the GraphQL API supports
cookie and OAuth 2.0 authentication. This means it's compatible with the OpenId module and supports JSON Web Token (JWT).

By default anonymous users are not able to execute a GraphQL query.

## Configuring Contents

When identifying content types for GraphQL exposure, we identify those without a stereotype to provide you with control over the behavior of stereotyped content types. A new option, `DiscoverableSterotypes`, has been introduced in `GraphQLContentOptions`. This allows you to specify stereotypes that should be discoverable by default.

For instance, if you have several content types stereotyped as `ExampleStereotype`, you can make them discoverable by incorporating the following code into the startup class:

```csharp
services.Configure<GraphQLContentOptions>(options =>
{
    options.DiscoverableSterotypes.Add("ExampleStereotype");
});
```

By utilizing the `GraphQLContentOptions`, you can customize the default visibility of content types, parts, or fields. For example, you have the option to conceal the author's name by configuring it as follows.

```csharp
services.Configure<GraphQLContentOptions>(options =>
{
    options.IgnoreField<ContentItemType>(nameof(ContentItem.Owner));
});
```

Or, hide a content type by default

```csharp
services.Configure<GraphQLContentOptions>(options =>
{
    options.ConfigureContentType("SiteLayers", x =>
    {
        x.Hidden = true;
    });
});
```

## Configuration

It's possible to configure graphql options for exposing exceptions and max depth, max complexity and field impact.

Configuration is done via the standard shell configuration, as follows.

```json
{
  "OrchardCore": {
    "OrchardCore_Apis_GraphQL": {
      "ExposeExceptions": true,
      "MaxDepth": 50, 
      "MaxComplexity": 100, 
      "FieldImpact": 2.0,
      "DefaultNumberOfResults": 100,
      "MaxNumberOfResults": 1000,
      "MaxNumberOfResultsValidationMode": "Default"
    }
  }
}
```

*ExposeExceptions (bool, Default: false for production, true for development)*

If set to true stack traces are exposed to graphql clients

*DefaultNumberOfResults (int, Default: 100)*
The default number of results returned by all paged fields/types.

*MaxNumberOfResults (int, Default: 1000)*
The maximum number of results returned by all paged fields/types.

*MaxNumberOfResultsValidationMode (enum, Values: Default|Enabled|Disabled, Default: Default)()*
Specify the validation behaviour if the max number of results is exceeded in a pager parameter

* Default - In production info will be logged and only the max number of results will be returned. In development a graphql validation error will be raised.
* Enabled - a graphql validation error will be raised
* Disabled - Info will be logged and only the max number of results will be returned

*MaxDepth (int?, Default: 20)*

Enforces the total maximum nesting across all queries in a request.

*MaxComplexity (int?, Default: null)*

*FieldImpact (double?, Default: null)*

For more information on MaxDepth, MaxComplexity, FieldImpact & protecting against malicious queries view the graphql-dot-net documentation at <https://graphql-dotnet.github.io/docs/getting-started/malicious-queries/>

## Use GraphQL from the CLI

`pomi graphql` sends GraphQL-over-HTTP requests directly to this module's endpoint.
It reuses the selected tenant context and its authentication. It does not wrap
GraphQL in a management REST endpoint, generate commands from OpenAPI, or fetch
OpenAPI metadata before executing a document. The GraphQL commands are built
into the CLI and are visible even when a tenant has not enabled GraphQL.

Enable `OrchardCore.Apis.GraphQL` on the tenant. Use the
[remote management guide](../../../guides/remote-management/README.md) to create
a context and log in, or use the configured client-credentials environment
variables. Normal CLI context/login setup uses Remote Management discovery;
the subsequent GraphQL request itself uses the existing GraphQL permissions.
It does not require `ViewOpenApiContent` or `AccessRemoteManagement`.

```bash
pomi graphql --help
pomi graphql execute --query '{ __typename }'
pomi graphql schema --output json > graphql-schema.json
pomi graphql schema --type SiteCulture
```

`schema` uses GraphQL introspection and returns the GraphQL JSON envelope, not
OpenAPI, JSON Schema, or SDL. `--type` selects one exact type name; an unknown
type returns `data.__type: null`. Types, fields, mutations, and introspection
availability depend on the tenant's features, schema configuration, and server
validation rules. `SiteCulture` is available when Localization contributes it.

For example, with Localization enabled:

```bash
pomi graphql execute --query '{ siteCultures { culture default } }'
```

### Documents, variables, and operation names

Use exactly one document source:

| Option | Input |
| --- | --- |
| `--query '<document>'` | Inline GraphQL document |
| `--file query.graphql` | UTF-8 GraphQL document file |
| `--stdin` | GraphQL document from standard input |
| `--named-query <name>` | An existing Orchard `INamedQueryProvider` query name |

Files and stdin contain **GraphQL text**, not the JSON HTTP request wrapper.
Named queries must be registered by a server provider; this option does not
refer to SQL/Lucene queries managed by `pomi queries`.

Create `inspect.graphql`:

```graphql
query InspectType($name: String!) {
  __type(name: $name) {
    name
    fields { name }
  }
}
```

Create `variables.json`:

```json
{ "name": "SiteCulture" }
```

Run it:

```bash
pomi graphql execute --file inspect.graphql \
  --variables-file variables.json --operation-name InspectType
```

Alternatively, use `--variables '{"name":"SiteCulture"}'` for a JSON object
inline, or pipe the document with `cat inspect.graphql | pomi graphql execute
--stdin --variables-file variables.json`. Only one variables source is allowed.
`--operation-name` chooses an operation inside a document containing several
operations. Single-quote inline documents in shells that expand `$variables`;
files avoid shell quoting issues. Use protected files for sensitive variables.

### Contexts, endpoints, and authorization

```bash
pomi --context production graphql execute --file query.graphql
pomi graphql execute --file query.graphql --endpoint custom/graphql
```

The default endpoint is `api/graphql`, relative to the exact tenant URL. A
context targeting `https://cms.example.com/blog/` sends requests to
`https://cms.example.com/blog/api/graphql`. Use `--endpoint` if the server's
`GraphQLSettings.Path` is customized. The CLI applies its existing tenant URL
checks and does not follow HTTP redirects with the request's credentials.

Queries and introspection require `ExecuteGraphQL`. Mutations additionally
require `ExecuteGraphQLMutations`, and fields/resolvers can impose further
permissions. The CLI sends the token from `pomi login`, refreshing it when
needed, or obtains a client-credentials token using `OC_CLIENT_ID` and
`OC_CLIENT_SECRET`. It does not change roles, permissions, or schema visibility.

### Results, errors, and mutations

The CLI sends a JSON `POST` with `query` (or `namedQuery`), optional `variables`,
and optional `operationName`, following the existing HTTP endpoint contract.
Both queries and mutations use `graphql execute`; it sends the supplied document
without interpreting its operation type or adding a confirmation prompt. Only
run mutation documents you intend to execute.

The default output is readable in a terminal and JSON when redirected. Use
`--output json` for automation. Introspection stays JSON by default. The complete
GraphQL envelope is retained in JSON output, including `data`, `errors`, and
`extensions`.

A nonempty `errors` array returns exit code **4**, including when the HTTP
status is `200`. In human output, readable error messages go to stderr and any
nonnull partial `data` goes to stdout. JSON output preserves the complete
response on stdout with a brief diagnostic on stderr. `--output none` suppresses the response
but still returns failure. Non-GraphQL HTTP failures use the CLI's normal API
error reporting. Invalid input returns a nonzero exit code before a request is
sent.

The CLI does not retry GraphQL requests automatically: a mutation might have
committed before a connection failed or another field reported an error.
Inspect the response and read back affected data before retrying. GraphQL
execution does not refresh or invalidate the OpenAPI cache; if a custom
mutation changes management commands, run `pomi api refresh` separately.

These commands support ordinary JSON HTTP requests. They do not implement
subscription streams, multipart uploads, or persisted-query hash negotiation.
For interactive query authoring, use the module's existing GraphiQL explorer.
