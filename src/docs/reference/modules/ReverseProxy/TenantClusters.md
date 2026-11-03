# Tenant Clustering

Tenant clustering lets Orchard Core resolve a tenant on the current node, map that tenant to a cluster slot, and forward the request to a YARP cluster chosen from configuration.

## When to use it

Use tenant clustering when:

- a single Orchard Core entry point should distribute tenant traffic across multiple backend nodes
- each tenant should consistently map to the same cluster
- idle tenant pipelines should be released automatically on backend nodes

## Service registration

Add a reference to the `OrchardCore.Clusters.Yarp` package, then register the YARP integration in `Program.cs`:

```csharp
builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup");

builder.Services
    .AddReverseProxy()
    .AddTenantClusters()
    .LoadFromConfig(builder.Configuration.GetSection("OrchardCore:Clusters"));
```

Add the routing middleware just before `UseOrchardCore()`, and the tenant clustering middleware first in the proxy pipeline:

```csharp
app.UseTenantClusterRouting();

app.UseOrchardCore();

app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline
        .UseTenantClusters()
        .UseSessionAffinity()
        .UseLoadBalancing()
        .UsePassiveHealthChecks();
});
```

Use the same `Program.cs` on all the nodes, including the backend nodes with `Enabled` set to `false`: the routing middleware removes the secret of the forwarded requests before running a tenant, and `AddTenantClusters()` also adds the service releasing the idle tenants.

The `OrchardCore.Cms.Web` application is already wired this way, but only when the `OrchardCore:Clusters` section exists on start, so that a site without clustering has no reverse proxy. On this application, every node should define this section, with at least `"Enabled": false` on a backend node, and adding it to a running node requires a restart.

## Configuration

Add an `OrchardCore:Clusters` section to `appsettings.json`:

```json
{
  "OrchardCore": {
    "Clusters": {
      "Enabled": false,
      "ProxySecret": "A long random value",
      "MaxIdleTime": "01:00:00",
      "Routes": {
        "RouteTemplate": {
          "ClusterId": "cluster1",
          "Match": {
            "Path": "{**catch-all}"
          }
        }
      },
      "Clusters": {
        "cluster1": {
          "SlotRange": [0, 5460],
          "Destinations": {
            "destination1": {
              "Address": "https://localhost:5101/"
            }
          }
        },
        "cluster2": {
          "SlotRange": [5461, 10921],
          "Destinations": {
            "destination1": {
              "Address": "https://localhost:5201/"
            }
          }
        },
        "cluster3": {
          "SlotRange": [10922, 16383],
          "Destinations": {
            "destination1": {
              "Address": "https://localhost:5301/"
            }
          }
        }
      }
    }
  }
}
```

## Settings

| Setting | Description |
| --- | --- |
| `Enabled` | When `true`, the current node behaves as a clustering proxy and forwards tenant requests to backend clusters. This can be changed while the node is running. |
| `ProxySecret` | The secret sent with the forwarded requests, see [Loop prevention](#loop-prevention). Required when `Enabled` is `true`. This can be changed while the node is running. |
| `MaxIdleTime` | Inactivity timeout used by the clustered tenant background task before releasing an idle tenant pipeline, which must be greater than zero. This is read from the configuration of the node running the tenant, so it should be set on the backend nodes. |
| `Routes.RouteTemplate` | The YARP route definition used as the catch-all route for clustered tenant forwarding. Required when `Enabled` is `true`. |
| `Clusters.{clusterId}.SlotRange` | Inclusive tenant slot range handled by the cluster. Tenants whose slot isn't in any range get a `503` response. |
| `Clusters.{clusterId}.Destinations` | Standard YARP destination list for that cluster. |

When `Enabled` is `true`, a node with an invalid configuration (a missing route or `ProxySecret`, no cluster, an invalid or overlapping `SlotRange`) fails to start. An invalid `MaxIdleTime` also prevents a node from starting when `Enabled` is `false`. If the configuration becomes invalid while the node is running, the error is logged and the last valid tenant placement is still used.

## How tenant selection works

1. Orchard Core resolves the incoming tenant with the running shell table.
2. The tenant's `TenantId` is hashed into a stable cluster slot.
3. `ShellSettingsExtensions.GetClusterId()` finds the configured cluster whose `SlotRange` contains that slot.
4. The request is marked with a `ClusterFeature`.
5. `ClustersRoutingMiddleware` runs the YARP route without entering the Orchard Core pipeline, so no tenant is built on the proxy.
6. `ClustersProxyMiddleware` reassigns the request to the matching YARP cluster, or returns a `503` response if YARP doesn't know this cluster.

The request is forwarded with its original `Host` header, so that the backend node resolves the same tenant, including the tenants mapped with a `RequestUrlHost`. Because of this, a backend reached over HTTPS must present a certificate valid for the host of the tenant, which is also the name sent to it during the TLS handshake.

If the scheme differs between the entry point and the backend, enable the [Reverse Proxy](README.md) feature on the tenants and select the `X-Forwarded-Proto` header, which is added by the proxy. Otherwise a tenant generates its redirections for the scheme of the backend, and a backend reached over HTTPS marks the cookies as secure, so a client reaching the entry point over HTTP can't sign in.

## Request limits and timeouts

The proxy doesn't limit the size of the requests by default, as a tenant decides its own limits, for instance the media upload size of an action. The limits of the backend node still apply. To limit the requests on the proxy, set the `MaxRequestBodySize` of `Routes.RouteTemplate` in bytes. Changing only this value requires restarting the node.

YARP cancels a forwarded request when the backend doesn't respond for 100 seconds, which then gets a `504` response. This also includes the time to build a tenant on its first request. Set the `HttpRequest.ActivityTimeout` of the cluster to allow more time, see the [YARP HTTP client configuration](https://learn.microsoft.com/aspnet/core/fundamentals/servers/yarp/http-client-config) documentation.

## What this feature does

Tenant clustering is a placement feature.

- It deterministically assigns each tenant to one configured cluster based on its `TenantId`.
- It is meant to distribute tenant execution across backend clusters instead of letting every public node build and hold every tenant pipeline.
- It does not by itself make every tenant active on every node.

High availability is provided inside a cluster through that cluster's YARP `Destinations`. If a cluster contains multiple healthy destinations and each destination can serve the same tenant data, YARP can load balance or fail over within that cluster.

## Multiple nodes for the same tenant

Yes. A tenant can be served by multiple nodes when its assigned cluster contains multiple YARP destinations.

- tenant-to-cluster assignment is still one-to-one
- cluster-to-destination routing can be one-to-many
- `UseLoadBalancing()` then chooses a healthy destination inside that cluster to receive the request

YARP health checks are not enabled by default. Without them, a destination that is down still receives its share of the requests, and these requests get a `502` response. Configure the `HealthCheck` of the cluster to stop using it, see the [YARP health checks](https://learn.microsoft.com/aspnet/core/fundamentals/servers/yarp/dests-health-checks) documentation.

This means the current design supports high availability for a tenant inside its assigned cluster, as long as every destination in that cluster can serve the same tenant.

### Example: one cluster, two backend nodes

```json
{
  "OrchardCore": {
    "Clusters": {
      "Enabled": true,
      "ProxySecret": "A long random value",
      "MaxIdleTime": "01:00:00",
      "Routes": {
        "RouteTemplate": {
          "ClusterId": "cluster-a",
          "Match": {
            "Path": "{**catch-all}"
          }
        }
      },
      "Clusters": {
        "cluster-a": {
          "SlotRange": [0, 16383],
          "HealthCheck": {
            "Active": {
              "Enabled": true,
              "Interval": "00:00:10",
              "Timeout": "00:00:05",
              "Path": "/"
            }
          },
          "Destinations": {
            "destination1": {
              "Address": "https://backend1.example.com/"
            },
            "destination2": {
              "Address": "https://backend2.example.com/"
            }
          }
        }
      }
    }
  }
}
```

With this configuration, every tenant maps to `cluster-a`, and YARP can route requests for the same tenant to either backend. When the active health check marks one backend as unhealthy, requests continue to the other destination in the same cluster.

If clustering is disabled, Orchard Core behaves as usual and any node that has the tenant configuration and data can serve that tenant directly.

## Loop prevention

The proxy route sets the `From-Clusters-Proxy` request header to the value of `ProxySecret`, replacing any value sent by the client. A node with `Enabled` set to `true` only runs the tenant of a request having this header with its own `ProxySecret`, and forwards any other request. This prevents proxy loops when a destination also has `Enabled` set to `true`, without letting a client run a tenant on a clustering proxy.

Use a long random value of visible ASCII characters, and set the same one on all the nodes having `Enabled` set to `true`. Prefer an environment variable or another secret store to the `appsettings.json` file. The header is removed before running a tenant, so the code of a tenant can't read it. A node with `Enabled` set to `false` doesn't use this secret.

!!! warning
    A node with `Enabled` set to `false` runs the tenants of any request that it receives, so don't expose the backend nodes publicly.

## Idle tenant release

`AddTenantClusters()` registers a service that checks the last request time recorded for each tenant pipeline running on the node. When the configured `MaxIdleTime` expires, Orchard Core releases that shell context so it can be rebuilt on the next request.

This check runs each minute, so a tenant is released up to one minute after `MaxIdleTime` expires. A tenant that didn't receive a request since it has been built isn't released.

The tenants admin list of the `Default` tenant shows which tenants are awake or asleep on the node serving this page. Each node only knows its own tenants, so in a cluster a tenant running on another node is shown as asleep.

The background tasks of a released tenant don't run until its next request, as is the case for any tenant that didn't receive a request since the application started. Don't set `MaxIdleTime` on tenants relying on scheduled work, such as scheduled publishing or timer workflow events.

When `BackgroundServiceOptions.ShellWarmup` is enabled, the background service builds all the tenants of the node and runs their background tasks, including a tenant which has just been released, so only its request pipeline is released. Don't enable it on a clustering proxy, as this node would then run all the tenants.

## Operational behavior

### Removing a node or cluster from configuration

When you change the slot ranges so that a tenant maps to another cluster, the tenant is not proactively migrated. Instead:

1. the clustering proxy starts forwarding that tenant to the new target cluster after the updated configuration is applied
2. the target backend builds the tenant pipeline lazily on the next request
3. the old backend instance is eventually released when it becomes idle and `MaxIdleTime` expires

This means tenant execution moves on demand, not through an eager restart or warmup process.

### Adding a node or cluster

Adding a new cluster and changing slot ranges does reshuffle tenants whose cluster slot now falls into the new range.

Nodes acting as the clustering proxy observe `OrchardCore:Clusters` changes dynamically. When the underlying configuration provider reloads the updated section, the running proxy starts using the new slot ranges and YARP destinations without restarting the process.

### Node failure without a configuration change

There is no automatic cross-cluster reassignment when a whole cluster becomes unavailable and the configuration is unchanged.

- If the selected cluster has multiple YARP destinations, `UseLoadBalancing()` and `UsePassiveHealthChecks()` can keep routing within that same cluster.
- If the selected cluster has no healthy destination left, requests for tenants assigned to that cluster fail until the cluster becomes healthy again or the clustering configuration is changed.

### Creating or changing a tenant

The clustering proxy places a tenant from its own list of tenants, which is not updated when a tenant is created, changed or removed through a backend node. Until the proxy gets the new list:

- The requests of a new tenant are forwarded to the cluster of the `Default` tenant, as the proxy falls back on it when it doesn't know the requested URL. Once the proxy knows this tenant, they are forwarded to the cluster matching its slot, which may be another one.
- The requests of a disabled or removed tenant are still forwarded to its cluster.

To keep all the nodes in sync, store the tenant list in a shared location with one of the [Shells Configuration](../Shells/README.md) providers, and enable the `OrchardCore.Tenants.Distributed` feature with a distributed cache such as `OrchardCore.Redis.Cache` on the `Default` tenant. A clustering proxy also needs this shared location, the database of the `Default` tenant and the Redis configuration, but it doesn't build any tenant pipeline. The proxy then gets the changes within a few seconds.

### Backend data requirements

Every backend destination that may receive a tenant request must be able to serve that tenant. In practice, nodes should share the same tenant definitions and have access to the same backing data, or an equivalent synchronized copy.

For production high availability, destinations inside the same cluster should typically share:

- the tenant database
- data protection keys
- media storage, if tenants use media
- any other persistent state required by enabled features

## See also

- [Tenant Clustering Internals](../../../topics/tenant-clustering/README.md)
- [Local Tenant Clustering Demo](../../../topics/tenant-clustering/local-demo.md)
- [Background Tasks](../BackgroundTasks/README.md)
- [Shells Configuration](../Shells/README.md)
- [Tenants](../Tenants/README.md)
