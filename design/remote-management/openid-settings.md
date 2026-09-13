# OpenID tenant settings

Three feature-owned settings sections expose explicit fields for the existing
server, client and validation settings models. Existing IOpenIdServerService,
IOpenIdClientService and IOpenIdValidationService remain the validation/persistence
owners. The shared client-secret editor is also used by the existing admin driver.
All updates require HTTPS and the corresponding existing permission, preserve
omissions, reject unknown fields, and request the existing tenant release only
when persisted values change.

Client secrets and extra authentication parameters are write-only. Null clears,
omission retains, and equivalent plaintext secrets preserve their ciphertext.
Extra parameters retain the existing storage semantics (not newly encrypted).
Readback includes only explicitly registered fields and presence flags. Private
keys, host configuration and custom application option overrides are outside this
contract. Existing certificates are selected through store metadata; no private
certificate material is imported/exported.

Focused tests cover secret protection/readback/clear/retries, invalid field and
shared-service validation, state preservation and reload behavior. Live HTTPS tests pass through HTTP/Pomi/MCP, including discovery changes after
reload, real service validation failures, secret redaction, retries, clearing,
and independent child-tenant configuration. After integration with merged user policies and custom user settings, the strict
full solution build and all 3,670 server tests passed (one skipped). Documentation
packaging and final CI remain required before merge.
