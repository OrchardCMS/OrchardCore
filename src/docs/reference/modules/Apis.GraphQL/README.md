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

`http://myapi/graphql?query={me{name}}`

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
