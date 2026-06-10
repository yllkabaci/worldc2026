---
paths:
  - "src/**/Observability/**/*.cs"
  - "src/**/*Telemetry*.cs"
  - "src/**/*Logging*.cs"
  - "src/WorldCup.Api/Program.cs"
---

# Observability

Logging, tracing, metrics, and health. Binding spec — the observability layer is reconstructable from this file. Three pillars: **structured logs (Serilog)**, **traces + metrics (OpenTelemetry)**, and **health checks**, tied together by a **correlation id**.

## Structured logging (Serilog)
- Serilog is the logging provider, configured via `Host.UseSerilog(...)` reading from configuration, enriched `FromLogContext`, console sink (JSON-friendly). Levels come from the `Serilog` config section.
- **Always structured, never interpolated:** `logger.LogInformation("User {UserId} predicted match {MatchId}", userId, matchId)` — never `$"..."`. Message templates are stable; values are properties.
- **Levels:** `Information` for normal flow and request completion; `Warning` for handled domain failures (4xx); `Error` for unhandled exceptions (5xx); `Debug` for dev detail. The global exception handler logs unhandled errors at `Error` and handled domain exceptions at `Warning` (see `error-codes.md`).
- **MediatR `LoggingBehavior`** logs each request name + outcome + elapsed ms (see `mediatr-pipeline.md`). Per-request HTTP logging uses `UseSerilogRequestLogging`, enriched with the correlation id and (when authenticated) the user id.
- **Never log secrets or PII:** no passwords (hashed or plain), no JWTs/tokens, no full email/payment data in plain text. Mask or omit.

## Correlation id
- Every request has a correlation id. A middleware reads the inbound **`X-Correlation-Id`** header (or generates a GUID if absent), pushes it onto the Serilog `LogContext` for the request, and echoes it back on the response **`X-Correlation-Id`** header.
- The correlation id flows into logs and trace context, and is propagated on outbound `HttpClient` calls (e.g. football-data.org) so a request can be followed end to end.

## Tracing & metrics (OpenTelemetry)
- OpenTelemetry is registered via `AddOpenTelemetry()` with a service resource named **`WorldCup.Api`**.
- **Tracing:** ASP.NET Core + `HttpClient` instrumentation, the app's own `ActivitySource` (`WorldCup.Api`), exported via **OTLP**. Add EF Core instrumentation when available.
- **Metrics:** ASP.NET Core + `HttpClient` + runtime instrumentation, exported via OTLP.
- **Custom spans:** wrap the **scoring engine** and **external provider calls** in activities from the shared `ActivitySource` (`Telemetry.Source`), tagging domain context (e.g. `match.id`, `prediction.count`). Custom span/tag names use a stable prefix (e.g. `wc.`). Spans must not carry secrets/PII.
- The OTLP endpoint is configuration-driven (default local collector). Absence of a collector must not break the app.

## Health checks
- **`/healthz`** — liveness: the process is up. No external dependencies checked.
- **`/readyz`** — readiness: dependencies reachable. Includes a **DbContext health check** (`AddDbContextCheck<ApplicationDbContext>` tagged `ready`); add an external-provider check when live football-data.org is enabled. `/readyz` filters to the `ready`-tagged checks.
- Health endpoints are anonymous (see `auth-and-authorization.md`).

## Where it lives
- `Api/Common/Observability/`: `CorrelationIdMiddleware`, `Telemetry` (the `ActivitySource`), and any enrichers. Serilog + OpenTelemetry + health registration live in `Program.cs`. Logging is cross-cutting — features add spans/log statements but do not re-wire the stack.

## Forbidden
No string-interpolated log messages; no secrets/tokens/PII in logs, spans, or tags; no `Console.WriteLine` for logging; no per-call `new` of loggers (inject `ILogger<T>`); no health check that performs heavy work or leaks dependency details to anonymous callers.
