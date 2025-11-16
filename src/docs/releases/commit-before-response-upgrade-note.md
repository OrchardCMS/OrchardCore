# Database Commit Lifecycle Change: Commit-Before-Response

## Summary

OrchardCore 2.x introduces a significant change to how database transactions are committed during HTTP requests. Previously, database commits occurred during scope disposal (after the HTTP response was sent). This has been replaced with a commit-before-response mechanism to prevent race conditions.

## Problem Description

The previous implementation registered a commit callback in `BeforeDispose` that executed during scope disposal, which happens **after** the request pipeline completes and the response has been sent to the client. This created a race condition where:

1. A request modifies data (e.g., creates an OAuth token, persisted grant, or authorization code)
2. The HTTP 200 response is sent to the client
3. The database commit happens asynchronously during scope disposal
4. A follow-up request from the client (e.g., OpenID Connect `/userinfo` after token issuance) may fail because the data hasn't been committed yet

## Solution

Database commits now occur **before** the HTTP response is sent, using ASP.NET Core's `Response.OnStarting` callback. This ensures that all database changes are durable before the client receives a success response.

## Changes Made

### 1. New Middleware: `CommitSessionMiddleware`

A new middleware automatically commits the `IDocumentStore` or `ISession` before the response starts:

- If `IDocumentStore` was resolved during the request, it calls `IDocumentStore.CommitAsync()`
- Otherwise, if `ISession` was resolved, it calls `ISession.SaveChangesAsync()`
- Commits run only once per request
- Commits are skipped if already executed
- **Commits are skipped if an exception occurs during request processing** to prevent committing potentially inconsistent state

### 2. Configuration Options: `EnsureCommittedOptions`

New configuration options control the commit behavior:

```json
{
  "OrchardCore_EnsureCommitted": {
    "Enabled": true,
    "FailureBehavior": "ThrowOnCommitFailure",
    "FlushOnPaths": []
  }
}
```

**Properties:**

- `Enabled` (bool, default: `true`): Enables or disables automatic commit-before-response
- `FailureBehavior` (enum, default: `ThrowOnCommitFailure`):
  - `ThrowOnCommitFailure`: Throws an exception when commit fails, preventing the response from being sent
  - `LogOnly`: Logs the error but allows the response to be sent
- `FlushOnPaths` (string[], default: `[]`): Optional list of path prefixes; when non-empty, only requests matching these paths will trigger commits

### 3. Removed BeforeDispose Handler

The previous `RegisterBeforeDispose` handler that committed changes during scope disposal has been removed from `AddDataAccess`.

## Migration Guide

### For Most Users

**No action required.** The change is enabled by default and maintains backward compatibility for typical use cases.

### To Disable (Not Recommended)

If you need to temporarily disable the new behavior:

```json
{
  "OrchardCore_EnsureCommitted": {
    "Enabled": false
  }
}
```

**Warning:** Disabling this feature re-introduces the race condition and is not recommended for production use.

### To Configure Commit Failure Behavior

By default, commit failures throw exceptions and abort the response. To log errors but allow the response to continue:

```json
{
  "OrchardCore_EnsureCommitted": {
    "FailureBehavior": "LogOnly"
  }
}
```

**Note:** Using `LogOnly` may result in data loss if commits fail silently. Use with caution and ensure proper monitoring.

### To Limit Commits to Specific Paths

For performance optimization or debugging, you can limit commits to specific request paths:

```json
{
  "OrchardCore_EnsureCommitted": {
    "FlushOnPaths": ["/api/", "/token", "/connect/"]
  }
}
```

## Best Practices

1. **Keep Enabled:** Leave `Enabled: true` (default) to ensure data consistency
2. **Explicit Commits:** For critical operations, continue to call `SaveChangesAsync()` explicitly where logically appropriate. The middleware acts as a safety net.
3. **Monitor Logs:** Watch for commit failures in your logs, especially if using `LogOnly` behavior
4. **Test Thoroughly:** Test your application to ensure the new commit timing doesn't introduce unexpected side effects
5. **Exception Handling:** The middleware automatically skips commits when exceptions occur during request processing, preventing inconsistent state from being persisted

## Exception Handling

The middleware is designed to handle exceptions gracefully:

- **No Commit on Exception:** If an exception occurs during request processing, the database commit is skipped automatically
- **Transaction Rollback:** This prevents potentially inconsistent or partial data from being committed
- **Error Responses:** Exceptions still propagate normally, ensuring proper error responses to clients
- **Data Integrity:** This behavior maintains data integrity by only committing when requests complete successfully

## Breaking Changes

### Non-HTTP Contexts

The new middleware only affects HTTP requests. Background jobs, shell initialization, and other non-HTTP contexts are **not affected** and continue to work as before.

### Custom Middleware Order

The commit middleware is registered via `IStartupFilter` and runs early in the pipeline. If you have custom middleware that:

- Modifies the response after database operations
- Relies on the old dispose-time commit behavior

You may need to adjust your middleware order or explicitly call `SaveChangesAsync()` in your code.

### Timing-Sensitive Code

If your application has code that depends on commits happening during disposal (e.g., deferred tasks that rely on uncommitted state), you'll need to refactor to:

- Explicitly commit before scheduling deferred tasks
- Adjust your logic to account for earlier commit timing

## Benefits

1. **Eliminates Race Conditions:** Client requests no longer fail due to uncommitted data
2. **Consistent State:** Ensures database state matches the HTTP response status
3. **Improved Reliability:** Follow-up requests (e.g., OAuth flows) work reliably
4. **Better Diagnostics:** Commit failures are detected before the response is sent, making debugging easier

## Impact on OpenID Connect

This change specifically addresses a critical issue in OpenID Connect flows where:

1. A client requests a token at `/connect/token`
2. OrchardCore creates a token and returns HTTP 200
3. The client immediately calls `/connect/userinfo` with the token
4. The `/userinfo` request fails because the token hasn't been committed yet

With the new middleware, the token is guaranteed to be committed before the `/connect/token` response is sent.

## Support

If you encounter issues after this change:

1. Check your logs for commit failures
2. Verify your configuration (ensure `Enabled: true`)
3. Review custom middleware that interacts with database sessions
4. Report issues at: https://github.com/OrchardCMS/OrchardCore/issues

## Related Pull Requests

- Implementation PR: [Link to be added]
- Related OpenID issue: [Link to be added]
