# Management API reference pages

Read this reference whenever adding or changing an Orchard Core OpenAPI management endpoint or its documentation.

## Workflow

1. Inventory every route in the owning endpoint mapping class.
2. Read request/response models, JSON attributes or naming policy, handler/service branches, OpenAPI metadata, endpoint tests, the underlying resource authorization checks, and any storage primitive that determines atomicity or conflict behavior.
3. Create or update `src/docs/reference/api/<resource>/README.md`.
4. Link the canonical page from the owning module README, `src/docs/reference/api/README.md`, and `mkdocs.yml`.
5. Run `python -m mkdocs build --strict`.

Never guess JSON casing, defaults, required fields, constraints, permissions, status codes, or retry behavior. The endpoint's coarse permission metadata may not be the complete authorization contract: document per-resource checks and the handling of resources the caller cannot access. Identify feature-dependent schema portions as dynamic.

## Required page shape

Use this sequence:

1. Purpose and owning/enabling feature.
2. Authentication and exact permissions.
3. Base route.
4. Endpoint summary table with method, path, description, and permission.
5. One section per operation:
   - exact method and route;
   - path, query, header, and body parameters with type, requirement, accepted values, default, and constraints;
   - exact request example or an explicit statement that no body is accepted;
   - every meaningful response status and representative response example;
   - for mutations, retry and idempotency semantics, equivalence criteria, atomicity or partial-progress behavior, and stable client-supplied identifiers when supported.
6. Paging envelope where applicable.
7. Shared errors and behavioral caveats.
8. Endpoint source files used to verify coverage.

Keep the endpoint contract normative in the API page. Module pages should describe the feature and link to the API page instead of duplicating route details.

## Mutation contract

For every `POST`, `PUT`, `PATCH`, and `DELETE`, state what happens when the same logical request is repeated:

- whether an identical retry succeeds and which status it returns;
- which request fields define equivalence and which differing fields produce `409 Conflict`;
- whether deletion and lifecycle transitions converge when the target is already in the requested state;
- whether destination creation or replacement is atomic under concurrent requests;
- whether a client-supplied stable identifier makes creation retry-safe;
- whether inaccessible related resources are preserved, rejected, or omitted;
- whether an operation can make partial progress and how a retry completes or repairs it.

Call out intentional exceptions explicitly. Operations that rely on server-generated identifiers, uploads without a stable identity, or commands that start a new execution may create a distinct resource or side effect on every successful request.

Do not describe an endpoint as idempotent merely because its HTTP verb is normally idempotent. Verify the implementation and tests.

Use the detailed contributor checklist in `src/docs/contributing/documenting-management-apis.md`.
