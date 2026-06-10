# 01 — Authentication (spine)

**Slices:** `Register`, `Login` · **Feature folder:** `Features/Authentication` · **Auth:** anonymous
**Consumes:** SPEC §4–§6, `backend-architecture.md §6`, `.claude/rules/*`.
**Build with:** the `create-feature` skill. Bonus identity features (OAuth, reset, verification) are tier 2.

## System Overview
Lets a visitor create an account and sign in, returning a JWT used for all protected requests. Local email/password only for the MVP; OAuth (Google/Facebook, UC-U02) and password reset (UC-U03) are deferred.

## Behavioral Contract
- When a visitor registers with a well-formed, unique email and a compliant password, the system creates an active `User` with role `User` and returns success.
- When a visitor registers with an email that already exists, the system rejects with `409` and creates nothing.
- When a user logs in with correct credentials, the system returns a **JWT** carrying `sub`, `email`, and `role` claims.
- When credentials are wrong, the system returns `401` and reveals nothing about which field failed.
- When any protected endpoint is called without a valid token, the system returns `401`.

## Explicit Non-Behaviors
- Must **not** store or log passwords in plaintext (hash per BR-017).
- Must **not** issue a token to a blocked account (BR-009); blocked accounts cannot log in.
- Must **not** include role-elevation in self-registration — new users are always `User`.

## Integration Boundaries
- `IJwtIssuer` (Infrastructure) mints tokens; password hasher hashes/verifies. No external OAuth provider in the MVP.

## Domain Notes
- `User` aggregate: email, hashed password, `Role` (User/Admin/SuperAdmin), `AccountStatus` (Active/Blocked). Factory rejects invalid email/password.

## Validation (input shape only)
- Email present and well-formed; password meets BR-017 (≥8 chars, ≥1 digit, ≥1 uppercase, ≥1 special). Business invariants (uniqueness) enforced in the domain/handler.

## Error Codes
`ValidationError` (400), `Conflict`/duplicate email (409), `Unauthorized` (401).

## Definition of Done
Register + login work end to end against the SQLite-backed factory; a protected endpoint accepts the issued token and rejects missing/invalid tokens. Verified by holdout scenarios (external).

## Resolved Decisions
1. Accounts are **active on registration** — no email-verification gate in the MVP (verification is tier 2).
2. **No lockout** in the MVP (BR-018 is tier 2).
3. JWT uses a **symmetric dev signing key**.
4. Runtime: **.NET 10 (LTS)**.
