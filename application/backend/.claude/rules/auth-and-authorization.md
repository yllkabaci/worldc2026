---
paths:
  - "src/**/Identity/**/*.cs"
  - "src/**/Features/Auth*/**/*.cs"
  - "src/**/Features/Authentication/**/*.cs"
  - "src/**/*Jwt*.cs"
  - "src/**/*CurrentUser*.cs"
---

# Authentication & Authorization

How identity and access control work. Binding spec — the auth layer should be reconstructable from this file. MVP is **minimal local auth** (email/password + JWT); OAuth, lockout, and email verification are tier 2.

## Authentication (JWT Bearer)
- The API authenticates with **JWT Bearer**. Tokens are minted by **`IJwtIssuer`** (Domain abstraction; `JwtIssuer` impl in Infrastructure) and carry claims: `sub` (user id), `email`, `role` (one claim per role), and a `jti`.
- Signing: **symmetric dev key** (`HS256`) from `JwtSettings` (`Issuer`, `Audience`, `SigningKey`, `ExpiryMinutes`). The key is a **secret** — supplied via config/env/user-secrets, **never committed**. Prod may move to asymmetric keys.
- Validation parameters (registered in `Program`): validate issuer, audience, signing key, and lifetime. Default inbound claim mapping is in effect (`sub` → `ClaimTypes.NameIdentifier`, `email` → `ClaimTypes.Email`).
- Passwords are hashed via **`IPasswordHasher`** (`BcryptPasswordHasher`). **Plaintext is never stored or logged.** Password shape (BR-017: ≥8 chars, ≥1 digit, ≥1 uppercase, ≥1 special) is checked in the validator (input shape); uniqueness/account state is enforced in the domain/handler.

## Registration & login (MVP)
- Register: create an **active** `User` with role `User` (email/password). Duplicate email → `409`. No email-verification gate in the MVP (tier 2).
- Login: verify the password hash; on success issue a JWT. Wrong credentials → `401` with no field disclosure. Self-registration never grants elevated roles.

## Current user in handlers
- Handlers read identity through **`ICurrentUserService`** (`UserId`, `Email`, `Roles`, `IsAuthenticated`) — backed by `IHttpContextAccessor` in the API. **Never** read claims from `HttpContext` inside a handler, and never thread a `UserId` through commands solely for identity (see `handler-no-httpcontext.md`). `CurrentUserService` reads both the raw (`sub`/`email`) and mapped (`ClaimTypes.*`) claims so it is robust to inbound-claim mapping.

## Authorization (policies)
- Three named policies, registered once: **`User`**, **`Admin`**, **`SuperAdmin`**, with a hierarchy — `User` is satisfied by `User|Admin|SuperAdmin`; `Admin` by `Admin|SuperAdmin`; `SuperAdmin` by `SuperAdmin`.
- Every protected endpoint declares `.RequireAuthorization("<policy>")` (see `minimal-api-endpoints.md`). The **only** legitimately anonymous endpoints are auth (register/login/oauth-callback) and health (`/healthz`, `/readyz`); Swagger UI is dev-only.
- Admin/Super-Admin operations are gated by the matching policy. Authorization decisions trust the **validated token's** role claims — never a client-supplied role/email beyond the token.
- Blocked accounts (BR-009) cannot authenticate or act; enforced in the domain/handler. Account lockout after 5 failed logins for 15 min (BR-018) and 24h session expiry (BR-019) are **tier 2**.

## Errors
- Unauthenticated → `401` (`ErrorCodes.Unauthorized`); forbidden (policy fails) → `403` (`ErrorCodes.Forbidden`). Mapped to RFC 7807 by the global handler (see `error-codes.md`). Auth failures reveal nothing about which field was wrong.

## OAuth (tier 2)
- Google/Facebook OAuth (UC-U02) and password reset (UC-U03) are deferred. When added: additional authentication handlers issue/link a local `User`, then the same `IJwtIssuer` mints the app JWT — policies and `ICurrentUserService` are unchanged.

## Testing
- Integration tests obtain a token via `IJwtIssuer` (or a test auth handler) and assert `200` with it and `401`/`403` without it / with the wrong role. Password hashing is exercised in unit tests; tokens/secrets/PII never appear in logs or test output. See `testing-conventions.md`.

## Forbidden
No plaintext password storage or logging; no logging of tokens/PII; no `AllowAnonymous` outside auth/health; no trusting client-supplied roles for privilege; no reading `HttpContext` in handlers; no committing the signing key.
