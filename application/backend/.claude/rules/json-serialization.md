---
paths:
  - "src/**/*.cs"
---

# JSON Serialization

ALWAYS use the shared `JsonDefaults` options. NEVER create ad-hoc `JsonSerializerOptions` per call site.

| Preset | Naming | Nulls | Enums | Use case |
|--------|--------|-------|-------|----------|
| `JsonDefaults.Api` | camelCase | omit | as strings (`JsonStringEnumConverter`) | API responses, ProblemDetails |

## Rules
- The API pipeline is configured **globally** via `ConfigureHttpJsonOptions` in startup - individual endpoints/handlers do not set options.
- **Enums serialize as strings**, not integers, so the React client and OpenAPI stay readable and stable.
- camelCase property names; null values are omitted from responses.
- Use the shared options for any manual (de)serialization: `JsonDefaults.Api`.
- **Exception**: do NOT force `JsonDefaults` when deserializing **external/third-party** JSON (e.g. the football API) - match that provider's contract with its own options.
- Strongly-typed IDs (see `domain-model-ddd.md`) serialize to their underlying value via a registered converter, not as a nested object.
