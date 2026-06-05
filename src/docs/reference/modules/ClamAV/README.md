# ClamAV (`OrchardCore.Antivirus.ClamAV`)

The ClamAV module scans files with a `clamd` service before Orchard Core stores or imports them.

When the feature is enabled, uploads fail closed:

- Malware detections are rejected before storage.
- Scanner connectivity, timeout, or protocol failures also reject the upload.
- The ClamAV connection is reused per configuration to avoid creating a new TCP client for every scan.

The scanner is wired through Orchard Core's file event handling abstractions, so uploads can be validated before storage without coupling media and deployment flows to a scanner-specific interface. ClamAV participates in that flow as an `IFileEventHandler`, and `FileEventService` aborts the upload when ClamAV returns a failed `FileCreatingResult`.

## Configuration

Configure the ClamAV connection in application configuration. The settings key remains `OrchardCore_Antivirus_ClamAV` for compatibility:

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
2. Enable the `ClamAV Antivirus Scanner` feature (`OrchardCore.Antivirus.ClamAV`).
3. Ensure a reachable `clamd` instance is running.

If the feature is enabled without a valid ClamAV connection, uploads are rejected until the scanner can verify them.

This feature integrates with the shared file upload security pipeline through `IFileEventHandler`, so uploads can be rejected before Orchard Core stores them permanently.

See [File Upload Security](../../core/file-upload-security.md) for the canonical guidance on invoking `FileEventService` in custom upload flows and aborting rejected files before they are stored.

## Notes

- The currently audited upload surfaces covered by this change are media uploads plus deployment package imports, both local and remote.
- Media uploads are scanned before storage because they flow through `DefaultMediaFileStore`.
- Deployment package zip/json uploads are scanned before Orchard Core writes the uploaded file to a temporary archive location for import.
