# ClamAV Antivirus Scanner (`OrchardCore.Antivirus.ClamAV`)

The ClamAV Antivirus Scanner module scans files with a `clamd` service before Orchard Core stores or imports them.

When the feature is enabled, uploads fail closed:

- Malware detections are rejected before storage.
- Scanner connectivity, timeout, or protocol failures also reject the upload.

The scanner is wired through the reusable `IAntivirusScanner` abstraction, so custom modules can replace it with another implementation when they need a different anti-virus provider.

## Configuration

Configure the ClamAV connection in application configuration:

```json
{
  "OrchardCore": {
    "OrchardCore_Antivirus_ClamAV": {
      "Host": "localhost",
      "Port": 3310,
      "ConnectTimeoutSeconds": 5,
      "TransferTimeoutSeconds": 30
    }
  }
}
```

The same settings can be provided with environment variables:

```text
OrchardCore__Antivirus_ClamAV__Host=localhost
OrchardCore__Antivirus_ClamAV__Port=3310
OrchardCore__Antivirus_ClamAV__ConnectTimeoutSeconds=5
OrchardCore__Antivirus_ClamAV__TransferTimeoutSeconds=30
```

## Usage

1. Configure the ClamAV settings.
2. Enable the `ClamAV Antivirus Scanner` feature.
3. Ensure a reachable `clamd` instance is running.

If the feature is enabled without a valid ClamAV connection, uploads are rejected until the scanner can verify them.

## Notes

- The currently audited upload surfaces covered by this change are media uploads plus deployment package imports, both local and remote.
- Media uploads are scanned before storage because they flow through `DefaultMediaFileStore`.
- Deployment package zip/json uploads are scanned before Orchard Core writes the uploaded file to a temporary archive location for import.
