# Run Orchard Core with the Aspire host

The Orchard Core source repository includes `OrchardCore.AspireHost`, a [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/) app host for local development. It starts the `OrchardCore.Cms.Web` project and a ClamAV container, then provides an Aspire dashboard where you can inspect their endpoints, logs, and status.

!!! warning
    `OrchardCore.AspireHost` is a development convenience, not a production deployment template. Review the container image, networking, persistence, signature updates, credentials, and observability requirements before adapting it for another environment.

## Prerequisites

Before you start the Aspire host, install or configure:

- The .NET SDK version selected by the repository's `global.json`.
- A Docker-compatible container runtime supported by .NET Aspire. Docker Desktop is the simplest option on Windows and macOS.
- A running container engine with access to Docker Hub so Aspire can pull `docker.io/clamav/clamav:latest`.
- Enough local disk space for the ClamAV image and the persistent `clamavdb` Docker volume.
- A trusted ASP.NET Core development certificate if you want to use the default HTTPS launch profile. Run `dotnet dev-certs https --trust` if needed.

Your workstation's endpoint protection can also inspect or quarantine files before Orchard Core receives them. If you test with an antivirus test file, follow your organization's security policy and check both the workstation antivirus and ClamAV logs when the result is unexpected.

## Start the host

From the repository root, run:

```bash
dotnet run --project src/OrchardCore.AspireHost
```

To avoid local HTTPS certificate requirements, use the HTTP launch profile:

```bash
dotnet run --project src/OrchardCore.AspireHost --launch-profile http
```

The command prints the Aspire dashboard URL. Open it, wait for the `antivirus` and `OrchardCoreCms` resources to start, then use the endpoint shown for `OrchardCoreCms` to set up or open the site.

The first start can take longer while Docker pulls the ClamAV image and initializes its virus definitions. Use the resource logs in the Aspire dashboard to follow this process.

## How the host works

The app host defines two resources in `src/OrchardCore.AspireHost/Program.cs`:

| Resource | Purpose |
| --- | --- |
| `antivirus` | Runs `docker.io/clamav/clamav:latest`, exposes the container's TCP port `3310` through a dynamically assigned host port, and persists `/var/lib/clamav` in the `clamavdb` Docker volume. |
| `OrchardCoreCms` | Runs `OrchardCore.Cms.Web`, exposes its HTTP endpoints, and receives the ClamAV host, port, and timeout settings through environment variables. |

Aspire resolves the ClamAV endpoint at run time. The app host maps that endpoint to these Orchard Core configuration keys:

```text
OrchardCore__Antivirus_ClamAV__Host
OrchardCore__Antivirus_ClamAV__Port
OrchardCore__Antivirus_ClamAV__ConnectTimeoutSeconds
OrchardCore__Antivirus_ClamAV__TransferTimeoutSeconds
```

The host sets `CLAMAV_NO_FRESHCLAMD=true`, so the container doesn't run the continuous `freshclam` update daemon. This keeps the local development setup simple, but it isn't an appropriate default for a production antivirus service.

## Enable antivirus scanning

Starting the Aspire host configures a reachable ClamAV service, but it doesn't enable antivirus scanning for a tenant.

After you complete the Orchard Core setup:

1. Sign in to the admin.
2. Go to **Configuration** > **Features**.
3. Enable **ClamAV Antivirus Scanner** (`OrchardCore.Antivirus.ClamAV`).
4. Upload a file and inspect the `OrchardCoreCms` and `antivirus` logs in the Aspire dashboard if the scan fails.

When the feature is enabled, Orchard Core rejects an upload if ClamAV detects malware or if the scanner can't complete the scan. See the [Antivirus module documentation](../reference/modules/Antivirus/README.md) for the covered upload flows and configuration details.

## Stop and restart the environment

Press `Ctrl+C` in the terminal that runs the app host. Aspire stops the resources it started. The `clamavdb` Docker volume remains available so ClamAV data can persist across runs.

Restart the same command to run the environment again. Orchard Core application data remains under `src/OrchardCore.Cms.Web/App_Data` unless you remove it separately.
