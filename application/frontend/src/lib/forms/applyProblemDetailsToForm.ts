import type { FieldValues, Path, UseFormSetError } from "react-hook-form";
import type { ProblemDetails } from "../api/problemDetails";

/**
 * Routes an RFC 7807 ProblemDetails onto a React Hook Form.
 *
 * - A `400` with an `errors` dictionary maps each field to an inline error.
 *   Backend property names are PascalCase (e.g. "Email"); they are normalised to the
 *   camelCase form field name ("email").
 * - Anything else (401/409/5xx, no `errors`) becomes a form-level `root` error.
 *
 * Domain conflicts that the UI wants on a specific field (e.g. a 409 "email already exists")
 * should be set by the caller before falling back to this helper.
 */
export function applyProblemDetailsToForm<T extends FieldValues>(
  problem: ProblemDetails,
  setError: UseFormSetError<T>,
): void {
  const entries = problem.errors ? Object.entries(problem.errors) : [];

  if (entries.length > 0) {
    for (const [key, messages] of entries) {
      const field = (key.charAt(0).toLowerCase() + key.slice(1)) as Path<T>;
      setError(field, { type: "server", message: messages.join(" ") });
    }
    return;
  }

  setError("root" as Path<T>, {
    type: "server",
    message: problem.detail ?? problem.title,
  });
}
