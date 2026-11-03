# Tenant Clustering

Tenant clustering lets Orchard Core resolve a tenant on the current node, map that tenant to a cluster slot, and forward the request to a YARP cluster chosen from configuration.

## When to use it

Use tenant clustering when:

- a single Orchard Core entry point should distribute tenant traffic across multiple backend nodes
- each tenant should consistently map to the same cluster
- idle tenant pipelines should be released automatically on backend nodes

## Service registration

Register Orchard Core, the inactivity check, and the YARP integration in `Program.cs`:

```csharp
builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup")
    .AddClusteredTenantInactivityCheck();

builder.Services
    .AddReverseProxy()
    .AddTenantClusters()
    .LoadFromConfig(builder.Configuration.GetSection("OrchardCore:Clusters"));
```

Place the tenant clustering middleware first in the proxy pipeline:

```csharp
app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline
        .UseTenantClusters()
        .UseSessionAffinity()
        .UseLoadBalancing()
        .UsePassiveHealthChecks();
});
```

## Configuration

Add an `OrchardCore:Clusters` section to `appsettings.json`:

```json
{
  "OrchardCore": {
    "Clusters": {
      "Enabled": false,
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
| `MaxIdleTime` | Inactivity timeout used by the clustered tenant background task before releasing an idle tenant pipeline. This is read from the configuration of the node running the tenant, so it should be set on the backend nodes. |
| `Routes.RouteTemplate` | The YARP route definition used as the catch-all route for clustered tenant forwarding. Required when `Enabled` is `true`. |
| `Clusters.{clusterId}.SlotRange` | Inclusive tenant slot range handled by the cluster. Tenants whose slot isn't in any range get a `503` response. |
| `Clusters.{clusterId}.Destinations` | Standard YARP destination list for that cluster. |

When `Enabled` is `true`, a node with an invalid configuration (a missing route, no cluster, an invalid or overlapping `SlotRange`) fails to start. If the configuration becomes invalid while the node is running, the error is logged and the last valid tenant placement is still used.

## How tenant selection works

1. Orchard Core resolves the incoming tenant with the running shell table.
2. The tenant's `TenantId` is hashed into a stable cluster slot.
3. `ShellSettingsExtensions.GetClusterId()` finds the configured cluster whose `SlotRange` contains that slot.
4. The request is marked with a `ClusterFeature`.
5. `ClustersProxyMiddleware` reassigns the request to the matching YARP cluster, or returns a `503` response if YARP doesn't know this cluster.

The request is forwarded with its original `Host` header, so that the backend node resolves the same tenant, including the tenants mapped with a `RequestUrlHost`. Because of this, a backend reached over HTTPS must present a certificate valid for the host of the tenant. If the scheme differs between the entry point and the backend, enable the [Reverse Proxy](README.md) feature on the tenants to apply the `X-Forwarded-Proto` header that is added by the proxy.

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

The proxy route adds the `From-Clusters-Proxy` request header. Incoming requests that already contain this header bypass cluster redistribution, which prevents proxy loops on backend nodes.

!!! warning
    This header isn't authenticated. A client sending it directly to a clustering proxy makes this node run the tenant itself, instead of forwarding the request. Remove this header from the incoming requests in the load balancer or gateway in front of the clustering proxy, and don't expose the backend nodes publicly.

## Idle tenant release

`AddClusteredTenantInactivityCheck()` registers a background task that checks the last request time recorded for each tenant pipeline. When the configured `MaxIdleTime` expires, Orchard Core releases that shell context so it can be rebuilt on the next request.

The background tasks of a released tenant don't run until its next request, as is the case for any tenant that didn't receive a request since the application started. Don't set `MaxIdleTime` on tenants relying on scheduled work, such as scheduled publishing or timer workflow events, nor when `BackgroundServiceOptions.ShellWarmup` is enabled, as the tenants would be released and built again each time `MaxIdleTime` expires.

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

To keep all the nodes in sync, store the tenant list in a shared location with one of the [Shells Configuration](../Shells/README.md) providers, and enable the `OrchardCore.Tenants.Distributed` feature on all the nodes, including the clustering proxy.

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
