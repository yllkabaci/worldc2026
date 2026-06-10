---
paths:
  - "src/**/Features/**/*Handler.cs"
  - "src/**/Features/**/*Validator.cs"
  - "src/**/Features/**/*Endpoint.cs"
  - "src/**/Domain/**/*.cs"
---

# Business Rule Placement

This service is the **system of record** for the prediction domain - it owns its truth. Business rules therefore live in the **domain**, not in endpoints, handlers, or validators. A business rule is any product decision about the domain: defaults with meaning, arithmetic on domain values, validation of invariants, lifecycle transitions, scoring, and audit handling.

## Placement framework

| Concern | Where it belongs |
|---------|------------------|
| Domain invariants & lifecycle (prediction allowed only before deadline, match state transitions, void rules) | **Domain** aggregates (methods with private setters) |
| Scoring arithmetic, void cases | **Domain** scoring service / `ScoringRuleSet` (see `scoring-engine.md`) |
| Configurable rules (point values, prediction window, group limits) | **Domain** config aggregate, **effective-dated** - never hardcoded constants |
| Orchestration (load aggregate -> invoke domain -> persist -> map response) | **Handler** (see `mediatr-cqrs.md`) |
| Input shape: types, required, ranges, format | **FluentValidation validator** (see below) |
| HTTP transport, status codes, auth policy | **Endpoint** (see `minimal-api-endpoints.md`) |

## The line between validators and the domain

**Validators check input shape only.** All business invariants live in the domain.

- **Validator (allowed):** required fields, types, value ranges (goals `0-20` per BR-010), string formats, enum membership.
- **Domain (required):** "prediction allowed only before the deadline" (BR-004/BR-007), "match must exist and be in a predictable state", "one active prediction per match" (BR-005), "points never negative" (BR-002), rule versioning.

Do **not** put stateful or cross-aggregate business checks in a validator (it has no transaction, no loaded aggregate, and can race). Those belong in the domain method that mutates state and throws a typed domain exception (see `error-codes.md`) when violated.

## Rules must be configurable, observable, and owned
- A rule that should vary (point values, prediction window) is **configuration data on a versioned aggregate**, not a hardcoded literal. Past matches are scored by the rule version active at settlement (business doc §8.2).
- When a business rule fires or is violated, it is **observable** (log/span) and surfaces a clear error - never a silent default that the API contract cannot predict.

## Checklist for every rule
1. Is it an invariant or arithmetic on domain values? -> Domain.
2. Is it merely input shape? -> Validator.
3. Should it vary by configuration/time? -> Versioned config aggregate, not a constant.
4. Is it observable when it fires? -> Add a log/span/clear error.
5. Could two requests race on it? -> Enforce inside the domain method under the transaction, not in a validator.
