# Documenting management APIs

Every OpenAPI management endpoint must be documented with the code that introduces or changes it. The owning feature's module documentation should link to one canonical page under `reference/api/`; do not maintain a second copy of the endpoint contract in the module page.

## Source of truth

Derive the reference from all of the following:

1. Endpoint mappings and their route, HTTP method, authorization, OpenAPI, and `x-oc-cli` metadata.
2. Request and response model types, including JSON serialization attributes and generated naming policy.
3. Handler and service behavior for defaults, validation, conflicts, and status codes.
4. The underlying resource authorization checks, including per-item permissions and the handling of related resources the caller cannot access.
5. Storage primitives that determine atomic creation, replacement, and concurrent conflict behavior.
6. Endpoint metadata and integration tests.

Do not infer casing, required fields, defaults, status codes, permissions, or retry behavior from a similar endpoint. Endpoint-level permission metadata can be only the first authorization gate; document additional per-resource checks and filtering. When a schema is tenant- or feature-dependent, document the stable envelope and explicitly identify the dynamic portion.

## Page location and navigation

Place the canonical page at:

```text
src/docs/reference/api/<resource>/README.md
```

Then:

1. Add it to the **Reference → API Reference** tree in `mkdocs.yml`.
2. Add an **API reference** link from the owning module page.
3. Add or update its entry in `reference/api/README.md`.

Use one page per cohesive resource. Split a large resource into child pages only when each page remains independently useful and the navigation groups them under the owning feature.

## Required page structure

Use this order consistently:

```markdown
# <Resource> management API

Purpose, owning feature, and enablement requirements.

## Authentication and permissions

Shared remote-management requirement and exact operation-specific permissions.

## Base route

`/<exact-prefix-from-the-endpoint-mapping>`

## Endpoints

| Method | Endpoint | Description | Permission |
| ... |

## <Operation>

`METHOD /exact/path`

Description and behavioral notes.

For a mutation, state whether and how the same logical request can be retried. Define equivalence and conflict rules, stable identifiers, convergent state transitions, atomicity guarantees, and partial-progress recovery. Explicitly identify operations that create a new server-generated resource or execution on every successful request.

### Parameters

| Location | Name | Type | Required | Description |
| ... |

### Request

Exact request example, or state that there is no request body.

### Responses

| Status | Meaning |
| ... |

Exact examples for every meaningfully different response representation.

## Errors

Shared validation, authorization, and conflict behavior.
```

For each parameter, include its exact serialized name, type, required or optional status, accepted values, default, and constraints. For request bodies, describe nested properties with the same information. For lists, document the paging parameters and complete response envelope.

Examples must use plausible values but exact shipped property names and shapes. Tag every code fence (`json`, `http`, `bash`, or another appropriate language).

## Operation checklist

Before submitting a change, verify:

- Every mapped management endpoint appears in the summary table and has an operation section.
- Routes, methods, operation IDs, feature requirements, authentication schemes, and permissions match endpoint metadata.
- Underlying resource permissions, per-item authorization, filtering, and preservation of inaccessible related resources are documented.
- Every handler-produced status code is documented, including empty responses.
- Request and response examples deserialize against the actual contracts.
- Paging, filtering, sorting, concurrency, and lifecycle semantics are explicit where supported.
- Every mutation documents repeated-request behavior: identical success, equivalence and conflict rules, stable identifiers, convergent deletes or transitions, and any intentional non-idempotent exception.
- Atomic destination semantics and partial-progress recovery are documented when storage or multi-step operations make them relevant.
- Dynamic or extensible schemas are identified rather than represented as closed contracts.
- The owning module page and API reference navigation link to the canonical page.
- `python -m mkdocs build --strict` succeeds.

When an endpoint changes incompatibly, update the reference in the same pull request and consider the compatibility impact on OpenAPI clients and `pomi` command metadata.
