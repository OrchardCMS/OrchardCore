import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom";
import GraphiQL from "graphiql";
import GraphiQLExplorer from "graphiql-explorer";
import { buildClientSchema, getIntrospectionQuery, IntrospectionQuery } from "graphql";
import { GraphiQLToolbarConfig } from "graphiql/dist/components/GraphiQL";

import "graphiql/graphiql.css";
import "../css/graphiql-orchard.css";

function getIntrospectionUrl(): string {
    return document
        .getElementById("graphiql")
        .dataset.introspectionUrl;
}

function fetcher(params: object): Promise<{ data?: unknown }> {
    const introspectionUrl = getIntrospectionUrl();
    return fetch(
        introspectionUrl,
        {
            method: "POST",
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            body: JSON.stringify(params),
            credentials: 'include'
        }
    )
    .then(function(response) {
        return response.text();
    })
    .then(function(responseBody) {
        try {
            return JSON.parse(responseBody);
        } catch {
            // Not valid JSON: display the raw response body as-is rather than an error.
            return responseBody as unknown as { data?: unknown };
        }
    });
}

function App() {
    // Gets a graphql query from the URL if present and sets it as the default query.
    const parameters = parseQueryFromUrl(window.location);

    const [query, setQuery] = useState(parameters.query);
    const [schema, setSchema] = useState(null);
    const [explorerIsOpen, setExplorerIsOpen] = useState(true);

    useEffect(() => {
        fetcher({
            query: getIntrospectionQuery()
        }).then(result => {
            setSchema(buildClientSchema(result.data as IntrospectionQuery));
        });
    }, []);

    function handleEditQuery(query: string) {
        setQuery(query);
        parameters.query = query;
        updateURL();
    }

    function handleToggleExplorer() {
        setExplorerIsOpen(!explorerIsOpen);
    }

    function onEditVariables(newVariables) {
        parameters.variables = newVariables;
        updateURL();
    }

    function onEditOperationName(newOperationName) {
        parameters.operationName = newOperationName;
        updateURL();
    }

    function parseQueryFromUrl(location: Location): Record<string, string> {
        const params: Record<string, string> = {};
        location.search.substr(1).split('&').forEach(function (entry) {
            const eq = entry.indexOf('=');
            if (eq >= 0) {
                params[decodeURIComponent(entry.slice(0, eq))] =
                    decodeURIComponent(entry.slice(eq + 1));
            }
        });
        // if variables was provided, try to format it.
        if (params.variables) {
            try {
                params.variables =
                    JSON.stringify(JSON.parse(params.variables), null, 2);
            } catch {
                // Do nothing, we want to display the invalid JSON as a string, rather
                // than present an error.
            }
        }

        return params;
    }

    function updateURL() {
        const newSearch = '?' + Object.keys(parameters).filter(function (key) {
            return Boolean(parameters[key]);
        }).map(function (key) {
            return encodeURIComponent(key) + '=' +
                encodeURIComponent(parameters[key]);
        }).join('&');
        history.replaceState(null, null, newSearch);
    }
     

    const btnToggleExplorer: GraphiQLToolbarConfig = {
        additionalContent: (
            <GraphiQL.Button
                onClick={handleToggleExplorer}
                label="Explorer"
                title="Toggle Explorer"
            />
        ),
    };

    return (
        <div className="graphiql-container">
            <GraphiQLExplorer
                schema={schema}
                query={query}
                onEdit={handleEditQuery}
                explorerIsOpen={explorerIsOpen}
                onToggleExplorer={handleToggleExplorer}
            />
            <GraphiQL
                // @ts-expect-error fetcher's return type is intentionally loose (see fetcher above); doesn't structurally match GraphiQL's Fetcher/SyncExecutionResult shape.
                fetcher={fetcher}
                schema={schema}
                variables={parameters.variables}
                operationName={parameters.operationName}
                onEditVariables={onEditVariables}
                onEditOperationName={onEditOperationName}
                query={query}
                onEditQuery={handleEditQuery}
                toolbar={btnToggleExplorer}
            ></GraphiQL>
        </div>
    );
}

ReactDOM.render(<App />, document.getElementById("graphiql"));
