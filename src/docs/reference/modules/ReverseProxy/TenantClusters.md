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
    .LoadFromConfig(builder.Configuration.GetSection("OrchardCore_Clusters"));
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

Add an `OrchardCore_Clusters` section to `appsettings.json`:

```json
{
  "OrchardCore_Clusters": {
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
```

## Settings

| Setting | Description |
| --- | --- |
| `Enabled` | When `true`, the current node behaves as a clustering proxy and forwards tenant requests to backend clusters. |
| `MaxIdleTime` | Global inactivity timeout used by the clustered tenant background task before releasing an idle tenant pipeline. |
| `Routes.RouteTemplate` | The YARP route definition used as the catch-all route for clustered tenant forwarding. |
| `Clusters.{clusterId}.SlotRange` | Inclusive tenant slot range handled by the cluster. |
| `Clusters.{clusterId}.Destinations` | Standard YARP destination list for that cluster. |

## How tenant selection works

1. Orchard Core resolves the incoming tenant with the running shell table.
2. The tenant's `TenantId` is hashed into a stable `ClusterSlot`.
3. `ShellSettingsExtensions.GetClusterId()` finds the configured cluster whose `SlotRange` contains that slot.
4. The request is marked with a `ClusterFeature`.
5. `ClustersProxyMiddleware` reassigns the request to the matching YARP cluster.

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
- `UseLoadBalancing()` and `UsePassiveHealthChecks()` then decide which healthy destination inside that cluster receives the request

This means the current design supports high availability for a tenant inside its assigned cluster, as long as every destination in that cluster can serve the same tenant.

### Example: one cluster, two backend nodes

```json
{
  "OrchardCore_Clusters": {
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
```

With this configuration, every tenant maps to `cluster-a`, and YARP can route requests for the same tenant to either backend. If one backend becomes unhealthy, requests can continue to the other healthy destination in the same cluster.

If clustering is disabled, Orchard Core behaves as usual and any node that has the tenant configuration and data can serve that tenant directly.

## Loop prevention

The proxy route adds the `From-Clusters-Proxy` request header. Incoming requests that already contain this header bypass cluster redistribution, which prevents proxy loops on backend nodes.

## Idle tenant release

`AddClusteredTenantInactivityCheck()` registers a background task that checks the last request time recorded for each tenant pipeline. When the configured `MaxIdleTime` expires, Orchard Core releases that shell context so it can be rebuilt on the next request.

## Operational behavior

### Removing a node or cluster from configuration

When you change the slot ranges so that a tenant maps to another cluster, the tenant is not proactively migrated. Instead:

1. the clustering proxy starts forwarding that tenant to the new target cluster after the updated configuration is applied
2. the target backend builds the tenant pipeline lazily on the next request
3. the old backend instance is eventually released when it becomes idle and `MaxIdleTime` expires

This means tenant execution moves on demand, not through an eager restart or warmup process.

### Adding a node or cluster

Adding a new cluster and changing slot ranges does reshuffle tenants whose `ClusterSlot` now falls into the new range.

In the current implementation, nodes acting as the clustering proxy observe `OrchardCore_Clusters` changes dynamically. When the underlying configuration provider reloads the updated section, the running proxy starts using the new slot ranges and YARP destinations without restarting the process.

### Node failure without a configuration change

There is no automatic cross-cluster reassignment when a whole cluster becomes unavailable and the configuration is unchanged.

- If the selected cluster has multiple YARP destinations, `UseLoadBalancing()` and `UsePassiveHealthChecks()` can keep routing within that same cluster.
- If the selected cluster has no healthy destination left, requests for tenants assigned to that cluster fail until the cluster becomes healthy again or the clustering configuration is changed.

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
- [Tenants](../Tenants/README.md)
