# Tenant Clustering Internals

This topic describes how the tenant clustering feature is wired into Orchard Core and which services it adds.

## Overview

Tenant clustering combines Orchard Core tenant resolution with YARP proxy routing:

1. the current node resolves the tenant from the incoming request
2. Orchard Core computes a stable cluster slot for that tenant
3. the request is associated with a target cluster
4. YARP forwards the request to that cluster
5. backend nodes execute the tenant pipeline and can release it after inactivity

The cluster is the placement boundary. A tenant maps to one cluster at a time, not to every cluster.

## Main components

| Component | Responsibility |
| --- | --- |
| `ShellSettings.ClusterSlot` | Computes a stable slot from the tenant identifier using CRC16/XMODEM. |
| `ShellSettingsExtensions.GetClusterId()` | Maps a tenant slot to a configured cluster ID. |
| `HttpContextExtensions.AsClustersProxy()` | Detects whether the current request should be forwarded by the clustering proxy. |
| `ClusterFeature` | Carries the selected cluster ID through the request pipeline. |
| `ClustersOptionsSetup` | Binds `OrchardCore_Clusters` configuration into `ClustersOptions`. |
| `ClustersProxyConfigFilter` | Updates the YARP route template and adds loop-prevention request headers. |
| `ClustersProxyMiddleware` | Reassigns a YARP proxy request to the selected cluster. |
| `ModularTenantContainerMiddleware` | Resolves the tenant, sets `ClusterFeature`, and bypasses normal tenant scope creation for proxy forwarding. |
| `ModularTenantRouterMiddleware` | Bypasses proxy-forwarded requests and records the last request timestamp for tenant pipelines. |
| `ClusteredTenantInactivityCheck` | Releases idle tenant shell contexts based on `MaxIdleTime`. |

## Request flow

### 1. Edge request

On a node acting as the clustering proxy, `ModularTenantContainerMiddleware` resolves the tenant first. When clustering applies, it computes the destination cluster ID and stores it in `ClusterFeature`.

### 2. Proxy routing

`UseTenantClusters()` runs first in the YARP pipeline. `ClustersProxyMiddleware` reads `ClusterFeature.ClusterId` and reassigns the request to the matching configured YARP cluster.

### 3. Loop prevention

`ClustersProxyConfigFilter` adds the `From-Clusters-Proxy` header to forwarded requests. Backend nodes use this header to skip redistribution and continue with normal tenant execution.

### 4. Backend tenant execution

On backend nodes, `ModularTenantRouterMiddleware` updates `ShellContext.LastRequestTimeUtc` before invoking the tenant pipeline. This keeps inactivity tracking aligned with real request traffic.

## Inactivity handling

`ClusteredTenantInactivityCheck` is a background task registered with `AddClusteredTenantInactivityCheck()`. It runs on a cron schedule and:

1. checks whether a tenant pipeline has already been built
2. reads `ShellContext.LastRequestTimeUtc`
3. compares it with the configured `MaxIdleTime`
4. releases the shell context when the timeout has expired

Releasing the shell context removes the cached tenant pipeline and service scope. Orchard Core rebuilds them on the next request.

## Placement and availability model

Tenant clustering is primarily a sharding mechanism for tenant execution:

- `ShellSettings.ClusterSlot` gives each tenant a stable placement key
- `ShellSettingsExtensions.GetClusterId()` chooses exactly one configured cluster for that key
- the proxy forwards the request to that cluster and backend execution happens there

This is different from running every tenant on every node.

If you need availability for a tenant, provide multiple YARP destinations inside the same cluster and make sure those destinations all have access to the same tenant configuration and data. In that model, YARP provides load balancing and passive failover within the tenant's assigned cluster.

## Reassignment behavior

When slot ranges change because you add or remove a cluster, affected tenants are reassigned by configuration. The move is lazy:

1. the next request is forwarded to the newly selected cluster
2. Orchard Core builds the tenant shell on that backend on demand
3. the previous backend instance remains until it is released, for example by `ClusteredTenantInactivityCheck`

There is no proactive tenant migration, no state copy performed by this feature, and no built-in warmup of the new backend pipeline.

When `OrchardCore_Clusters` is backed by a configuration provider that supports reload notifications, the running clustering proxy observes those changes dynamically. In that case, adding or removing clusters changes tenant placement on the next request without restarting the proxy node.

## Configuration assumptions

- clustering is configured through `OrchardCore_Clusters`
- tenant slot ranges are inclusive
- `MaxIdleTime` is a global setting for the clustering configuration
- the route identified by `ClustersOptions.RouteTemplate` is the catch-all route used for clustered forwarding

## Operational notes

- keep `UseTenantClusters()` first in the proxy pipeline so the cluster assignment happens before session affinity or load balancing
- choose non-overlapping `SlotRange` values
- use a shared clustering configuration across proxy and backend nodes
- keep `Enabled` set only on nodes that should actively redistribute tenant requests
- make sure every backend destination that may receive a tenant request can serve that tenant data
- use a configuration provider for `OrchardCore_Clusters` that supports reload notifications if you want placement changes to apply dynamically

## Failure behavior

Without a configuration change, the feature does not automatically move a tenant to a different cluster.

- If a destination inside the selected cluster fails and other destinations remain healthy in that same cluster, YARP can continue routing there.
- If the whole selected cluster becomes unavailable, requests for tenants assigned to that cluster fail until the cluster is restored or the clustering configuration is changed.

## Related documentation

- [Tenant Clustering](../../reference/modules/ReverseProxy/TenantClusters.md)
- [Local Tenant Clustering Demo](local-demo.md)
- [Reverse Proxy](../../reference/modules/ReverseProxy/README.md)
- [Tenants](../../reference/modules/Tenants/README.md)
