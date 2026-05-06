# Local Tenant Clustering Demo

This article shows how to reproduce the local 4-node demonstration used for end-to-end validation of tenant clustering.

The demo uses:

- one edge node that acts as the clustering proxy
- three backend nodes that execute tenant pipelines
- the same `OrchardCore.Cms.Web.dll` for every node
- a separate content root for each node so `App_Data`, logs, and node-specific `appsettings.json` stay isolated
- one SaaS root site and six tenants: `alpha`, `bravo`, `charlie`, `delta`, `echo`, and `foxtrot`

The exact tenant-to-cluster placement depends on the generated `TenantId` values. The diagrams below show one real validation run, so a fresh local run may not produce the exact same tenant placement.

## Topology

```text
                   Browser / curl
                         |
                         v
              +----------------------+
              | edge  :55797         |
              | Enabled = true       |
              | clustering proxy     |
              +----------------------+
                   |      |      |
          cluster-a|      |cluster-b
                   |      |
                   v      v
       +----------------+  +----------------+
       | backend1:55798 |  | backend2:55799 |
       | Enabled = false|  | Enabled = false|
       +----------------+  +----------------+
                          cluster-c
                              |
                              v
                    +----------------+
                    | backend3:55800 |
                    | Enabled = false|
                    +----------------+
```

## What this demo proves

This demo is useful when you want to verify all of the following on a local machine:

- one edge node can route tenants to multiple backend clusters
- adding a cluster reshuffles tenants whose slots move into the new range
- removing a cluster reshuffles tenants out of the removed range
- `OrchardCore_Clusters` changes are picked up dynamically by the running edge node
- tenant pipelines are built lazily on the backend that receives the next request
- a tenant can still be served when one destination in its assigned cluster goes down, if another healthy destination remains in that cluster

## Prerequisites

- a local clone of the Orchard Core repository
- .NET SDK 10 or later
- either Bash with `rsync`, or PowerShell
- four terminals

The commands below assume you run them from the repository root.

## 1. Confirm the host application wiring

If you use the `OrchardCore.Cms.Web` project from this repository, the required wiring is already in place and you do not need to change `Program.cs`.

If you want to reproduce the demo in another Orchard Core host application, make sure its `Program.cs` includes the same clustering and YARP registration pattern:

```csharp
using OrchardCore.Clusters;
using OrchardCore.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLogHost();

builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup")
    .AddClusteredTenantInactivityCheck();

builder.Services
    .AddReverseProxy()
    .AddTenantClusters()
    .LoadFromConfig(builder.Configuration.GetSection("OrchardCore_Clusters"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseOrchardCore();

app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline
        .UseTenantClusters()
        .UseSessionAffinity()
        .UseLoadBalancing()
        .UsePassiveHealthChecks();
});

await app.RunAsync();
```

## 2. Build once

=== "Bash"

    ```bash
    dotnet build src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj -c Debug -f net10.0
    ```

=== "PowerShell"

    ```powershell
    dotnet build src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj -c Debug -f net10.0
    ```

## 3. Create one working root per node

All nodes run the same compiled application. The only thing that changes per node is the content root and its local state.

=== "Bash"

    ```bash
    export OC_REPO="$PWD"
    export OC_DEMO="$OC_REPO/.demo/tenant-clustering"
    export OC_DLL="$OC_REPO/src/OrchardCore.Cms.Web/bin/Debug/net10.0/OrchardCore.Cms.Web.dll"

    mkdir -p "$OC_DEMO"

    for node in edge backend1 backend2 backend3; do
      rsync -a --delete \
        --exclude bin \
        --exclude obj \
        --exclude App_Data \
        "$OC_REPO/src/OrchardCore.Cms.Web/" \
        "$OC_DEMO/$node-root/"

      mkdir -p "$OC_DEMO/$node-root/App_Data"
    done
    ```

=== "PowerShell"

    ```powershell
    $OC_REPO = (Get-Location).Path
    $OC_DEMO = Join-Path $OC_REPO ".demo/tenant-clustering"
    $OC_DLL = Join-Path $OC_REPO "src/OrchardCore.Cms.Web/bin/Debug/net10.0/OrchardCore.Cms.Web.dll"

    New-Item -ItemType Directory -Force -Path $OC_DEMO | Out-Null

    foreach ($node in "edge", "backend1", "backend2", "backend3")
    {
        $source = Join-Path $OC_REPO "src/OrchardCore.Cms.Web"
        $target = Join-Path $OC_DEMO "$node-root"

        if (Test-Path $target)
        {
            Remove-Item $target -Recurse -Force
        }

        New-Item -ItemType Directory -Force -Path $target | Out-Null

        Get-ChildItem $source -Force |
            Where-Object { $_.Name -notin "bin", "obj", "App_Data" } |
            ForEach-Object { Copy-Item $_.FullName -Destination $target -Recurse -Force }

        New-Item -ItemType Directory -Force -Path (Join-Path $target "App_Data") | Out-Null
    }
    ```

Each copied root already contains its own copy of:

- `appsettings.json`
- `NLog.config`
- all other host files from `src/OrchardCore.Cms.Web/`

The copy command excludes only `bin`, `obj`, and `App_Data`.

That means the demo uses two different configuration locations:

| File location | Purpose in this demo |
| --- | --- |
| `<node>-root/appsettings.json` | Host-level configuration for that node. Edit this file for `Logging` and `OrchardCore_Clusters`. |
| `<node>-root/NLog.config` | Shared logging target definition copied into each node root. You do not need to edit it for this demo. |
| `<node>-root/App_Data/Sites/Default/appsettings.json` and `<node>-root/App_Data/Sites/<tenant>/appsettings.json` | Tenant state created by Orchard during setup. These files are copied later from the setup node, but they are not where you configure clustering for the demo. |

Because every process starts with a different `--contentRoot`, each node reads the `appsettings.json` from its own copied root, not from the central source tree.

## 4. Make request routing visible in each node log

Before starting any long-running demo nodes, edit the root-level `appsettings.json` in each copied root:

- `$OC_DEMO/edge-root/appsettings.json`
- `$OC_DEMO/backend1-root/appsettings.json`
- `$OC_DEMO/backend2-root/appsettings.json`
- `$OC_DEMO/backend3-root/appsettings.json`

In each of those files, keep `Microsoft.Hosting.Lifetime` at `Information` and also enable request diagnostics:

```json
"Logging": {
  "LogLevel": {
    "Default": "Warning",
    "Microsoft.Hosting.Lifetime": "Information",
    "Microsoft.AspNetCore.Hosting.Diagnostics": "Information"
  }
}
```

This makes it easy to see which backend received `GET /alpha`, `GET /bravo`, and so on.

The log files are then written under each node's own `App_Data/logs/` directory because each node has its own content root and its own `App_Data`.

## 5. Start a setup node with clustering disabled

For the setup phase, start only the edge root with clustering disabled.

At this point, `edge-root/appsettings.json` should either have no `OrchardCore_Clusters` section yet, or keep `"Enabled": false`.

Use `http://127.0.0.1:55797` for the setup node:

=== "Bash"

    ```bash
    dotnet "$OC_DLL" \
      --urls "http://127.0.0.1:55797" \
      --contentRoot "$OC_DEMO/edge-root"
    ```

=== "PowerShell"

    ```powershell
    dotnet $OC_DLL `
      --urls "http://127.0.0.1:55797" `
      --contentRoot (Join-Path $OC_DEMO "edge-root")
    ```

Open `http://127.0.0.1:55797/` and complete setup with:

- **Recipe**: `SaaS`
- **Site name**: `Cluster Live Reload`
- **User name**: `admin`
- **Email**: `admin@orchard.com`
- **Password**: `Orchard1!`

Then sign in and create six tenants from **Admin -> Tenants**:

| Tenant | Prefix | Recipe |
| --- | --- | --- |
| `alpha` | `alpha` | `Blog` |
| `bravo` | `bravo` | `Blog` |
| `charlie` | `charlie` | `Blog` |
| `delta` | `delta` | `Blog` |
| `echo` | `echo` | `Blog` |
| `foxtrot` | `foxtrot` | `Blog` |

Set up each tenant with the same credentials:

- **User name**: `admin`
- **Email**: `admin@orchard.com`
- **Password**: `Orchard1!`

## 6. Stop the setup node and copy its `App_Data` to the backends

After the root site and tenants are created, stop the setup node and fan out its state:

=== "Bash"

    ```bash
    rsync -a --delete "$OC_DEMO/edge-root/App_Data/" "$OC_DEMO/backend1-root/App_Data/"
    rsync -a --delete "$OC_DEMO/edge-root/App_Data/" "$OC_DEMO/backend2-root/App_Data/"
    rsync -a --delete "$OC_DEMO/edge-root/App_Data/" "$OC_DEMO/backend3-root/App_Data/"
    ```

=== "PowerShell"

    ```powershell
    foreach ($node in "backend1", "backend2", "backend3")
    {
        $target = Join-Path $OC_DEMO "$node-root/App_Data"

        if (Test-Path $target)
        {
            Get-ChildItem $target -Force | Remove-Item -Recurse -Force
        }

        Copy-Item (Join-Path $OC_DEMO "edge-root/App_Data/*") $target -Recurse -Force
    }
    ```

This step copies only `App_Data`, which means tenant definitions, recipes, SQLite databases if you use them, media, and the tenant-scoped `App_Data/Sites/.../appsettings.json` files created during setup.

It does **not** replace the root-level `appsettings.json` that was already copied into each node root in step 3.

## 7. Start the 4-node demo

Use four terminals.

These are the exact command lines used to start each node from the command line. Every process runs the same `OrchardCore.Cms.Web.dll`; only the port and content root change.

If you open new terminals, rerun the variable setup from step 3 in each terminal, or replace `OC_DLL` and `OC_DEMO` with the full paths.

### Terminal 1 - edge

=== "Bash"

    ```bash
    dotnet "$OC_DLL" \
      --urls "http://127.0.0.1:55797" \
      --contentRoot "$OC_DEMO/edge-root"
    ```

=== "PowerShell"

    ```powershell
    dotnet $OC_DLL `
      --urls "http://127.0.0.1:55797" `
      --contentRoot (Join-Path $OC_DEMO "edge-root")
    ```

### Terminal 2 - backend1

=== "Bash"

    ```bash
    dotnet "$OC_DLL" \
      --urls "http://127.0.0.1:55798" \
      --contentRoot "$OC_DEMO/backend1-root"
    ```

=== "PowerShell"

    ```powershell
    dotnet $OC_DLL `
      --urls "http://127.0.0.1:55798" `
      --contentRoot (Join-Path $OC_DEMO "backend1-root")
    ```

### Terminal 3 - backend2

=== "Bash"

    ```bash
    dotnet "$OC_DLL" \
      --urls "http://127.0.0.1:55799" \
      --contentRoot "$OC_DEMO/backend2-root"
    ```

=== "PowerShell"

    ```powershell
    dotnet $OC_DLL `
      --urls "http://127.0.0.1:55799" `
      --contentRoot (Join-Path $OC_DEMO "backend2-root")
    ```

### Terminal 4 - backend3

=== "Bash"

    ```bash
    dotnet "$OC_DLL" \
      --urls "http://127.0.0.1:55800" \
      --contentRoot "$OC_DEMO/backend3-root"
    ```

=== "PowerShell"

    ```powershell
    dotnet $OC_DLL `
      --urls "http://127.0.0.1:55800" `
      --contentRoot (Join-Path $OC_DEMO "backend3-root")
    ```

## 8. Initial layout: two clusters

Before starting or probing the full 4-node layout, edit only `edge-root/appsettings.json` and set its `OrchardCore_Clusters` section to:

```json
"OrchardCore_Clusters": {
  "Enabled": true,
  "MaxIdleTime": "00:00:20",
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
      "SlotRange": [0, 7314],
      "Destinations": {
        "destination1": {
          "Address": "http://127.0.0.1:55798/"
        }
      }
    },
    "cluster-b": {
      "SlotRange": [7315, 16383],
      "Destinations": {
        "destination1": {
          "Address": "http://127.0.0.1:55799/"
        }
      }
    }
  }
}
```

For this step:

- `edge-root/appsettings.json` contains the full `OrchardCore_Clusters` section shown below with `"Enabled": true`
- `backend1-root/appsettings.json`, `backend2-root/appsettings.json`, and `backend3-root/appsettings.json` keep clustering disabled

The backend files can either omit the `OrchardCore_Clusters` section entirely or contain the same section with `"Enabled": false`. The important part is that only the edge node actively redistributes requests.

Send one request per tenant through the edge:

=== "Bash"

    ```bash
    for tenant in alpha bravo charlie delta echo foxtrot; do
      curl -fsS "http://127.0.0.1:55797/${tenant}/" > /dev/null
      echo "requested ${tenant}"
    done
    ```

=== "PowerShell"

    ```powershell
    foreach ($tenant in "alpha", "bravo", "charlie", "delta", "echo", "foxtrot")
    {
        $null = Invoke-WebRequest -Uri "http://127.0.0.1:55797/$tenant/" -UseBasicParsing
        Write-Host "requested $tenant"
    }
    ```

In the validated run, requests landed like this:

```text
Step 1: initial 2-cluster layout

edge (:55797)
  cluster-a [0..7314]   -> backend1 (:55798) -> bravo, charlie
  cluster-b [7315..16383] -> backend2 (:55799) -> alpha, delta, echo, foxtrot

backend3 (:55800) is unused
```

## 9. Add a third cluster live

Now edit only `edge-root/appsettings.json` and change the cluster section to:

```json
"OrchardCore_Clusters": {
  "Enabled": true,
  "MaxIdleTime": "00:00:20",
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
      "SlotRange": [0, 3301],
      "Destinations": {
        "destination1": {
          "Address": "http://127.0.0.1:55798/"
        }
      }
    },
    "cluster-b": {
      "SlotRange": [3302, 7503],
      "Destinations": {
        "destination1": {
          "Address": "http://127.0.0.1:55799/"
        }
      }
    },
    "cluster-c": {
      "SlotRange": [7504, 16383],
      "Destinations": {
        "destination1": {
          "Address": "http://127.0.0.1:55800/"
        }
      }
    }
  }
}
```

Wait a few seconds so the configuration provider reloads the file, then probe again:

=== "Bash"

    ```bash
    for tenant in alpha bravo charlie delta echo foxtrot; do
      curl -fsS "http://127.0.0.1:55797/${tenant}/?probe=liveadd-${tenant}" > /dev/null
      echo "requested ${tenant}"
    done
    ```

=== "PowerShell"

    ```powershell
    foreach ($tenant in "alpha", "bravo", "charlie", "delta", "echo", "foxtrot")
    {
        $null = Invoke-WebRequest -Uri "http://127.0.0.1:55797/$tenant/?probe=liveadd-$tenant" -UseBasicParsing
        Write-Host "requested $tenant"
    }
    ```

In the validated run, the mapping changed immediately without restarting the edge:

```text
Step 2: add cluster-c live

edge (:55797)
  cluster-a [0..3301]     -> backend1 (:55798) -> charlie
  cluster-b [3302..7503]  -> backend2 (:55799) -> bravo
  cluster-c [7504..16383] -> backend3 (:55800) -> alpha, delta, echo, foxtrot
```

## 10. Remove cluster-b live

Edit only `edge-root/appsettings.json` again:

```json
"OrchardCore_Clusters": {
  "Enabled": true,
  "MaxIdleTime": "00:00:20",
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
      "SlotRange": [0, 7503],
      "Destinations": {
        "destination1": {
          "Address": "http://127.0.0.1:55798/"
        }
      }
    },
    "cluster-c": {
      "SlotRange": [7504, 16383],
      "Destinations": {
        "destination1": {
          "Address": "http://127.0.0.1:55800/"
        }
      }
    }
  }
}
```

Wait for reload, then probe again:

=== "Bash"

    ```bash
    for tenant in alpha bravo charlie delta echo foxtrot; do
      curl -fsS "http://127.0.0.1:55797/${tenant}/?probe=remove-${tenant}" > /dev/null
      echo "requested ${tenant}"
    done
    ```

=== "PowerShell"

    ```powershell
    foreach ($tenant in "alpha", "bravo", "charlie", "delta", "echo", "foxtrot")
    {
        $null = Invoke-WebRequest -Uri "http://127.0.0.1:55797/$tenant/?probe=remove-$tenant" -UseBasicParsing
        Write-Host "requested $tenant"
    }
    ```

In the validated run, backend2 stopped receiving tenant traffic without any restart:

```text
Step 3: remove cluster-b live

edge (:55797)
  cluster-a [0..7503]     -> backend1 (:55798) -> bravo, charlie
  cluster-c [7504..16383] -> backend3 (:55800) -> alpha, delta, echo, foxtrot

backend2 (:55799) no longer receives tenant requests
```

## 11. How to verify the demo

You can verify the behavior in three ways:

1. Watch the backend terminals. Each request appears in the node that actually handled it.
2. Confirm that editing only `edge-root/appsettings.json` changes placement after the next request.
3. Compare the requests you see with the placement diagrams shown for each reconfiguration step.

## 12. Optional: demonstrate failover inside one cluster

The previous steps showed tenant placement across clusters. This final step shows the other capability of the feature: the same tenant can be served by multiple backend nodes when its assigned cluster has multiple destinations.

Edit only `edge-root/appsettings.json` and change the cluster section to:

```json
"OrchardCore_Clusters": {
  "Enabled": true,
  "MaxIdleTime": "00:00:20",
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
          "Address": "http://127.0.0.1:55798/"
        },
        "destination2": {
          "Address": "http://127.0.0.1:55799/"
        }
      }
    }
  }
}
```

Wait for reload, then send a few requests for the same tenant through the edge:

=== "Bash"

    ```bash
    for i in 1 2 3 4 5; do
      curl -fsS "http://127.0.0.1:55797/alpha/?probe=ha-before-${i}" > /dev/null
      echo "requested alpha (${i})"
    done
    ```

=== "PowerShell"

    ```powershell
    foreach ($i in 1..5)
    {
        $null = Invoke-WebRequest -Uri "http://127.0.0.1:55797/alpha/?probe=ha-before-$i" -UseBasicParsing
        Write-Host "requested alpha ($i)"
    }
    ```

At this point, `alpha` can be served by either `backend1` or `backend2`, because both nodes are destinations of the same cluster.

Now stop `backend1`, for example by pressing `Ctrl+C` in the backend1 terminal, then probe the same tenant again:

=== "Bash"

    ```bash
    for i in 1 2 3 4 5; do
      curl -fsS "http://127.0.0.1:55797/alpha/?probe=ha-after-${i}" > /dev/null
      echo "requested alpha (${i}) after stopping backend1"
    done
    ```

=== "PowerShell"

    ```powershell
    foreach ($i in 1..5)
    {
        $null = Invoke-WebRequest -Uri "http://127.0.0.1:55797/alpha/?probe=ha-after-$i" -UseBasicParsing
        Write-Host "requested alpha ($i) after stopping backend1"
    }
    ```

You should still see successful responses through the edge, but now they should land on `backend2`.

```text
Step 4: high availability inside cluster-a

Before stopping backend1:
  cluster-a [0..16383] -> backend1 (:55798) or backend2 (:55799)

After stopping backend1:
  cluster-a [0..16383] -> backend2 (:55799)
```

This demonstrates failover inside one cluster. It does not demonstrate automatic reassignment to another cluster.

## Notes

- This demo intentionally uses the same built application for every node.
- The nodes still need separate content roots because `App_Data`, logs, and node-specific configuration must not be shared as one writable directory.
- In this demo, all clustering changes are made in the root-level `appsettings.json` of the edge node. The tenant `App_Data/Sites/.../appsettings.json` files are setup output, not the place where the cluster map is edited.
- For a production-style setup, nodes in the same cluster should share backing services such as the tenant database, data protection keys, and media storage, while still keeping node-local runtime files separate.
- If your configuration provider does not support reload notifications, placement changes will not apply dynamically until the edge process restarts.

## Related documentation

- [Tenant Clustering](../../reference/modules/ReverseProxy/TenantClusters.md)
- [Tenant Clustering Internals](README.md)
