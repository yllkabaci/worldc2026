/** Mirrors the backend RFC 7807 ProblemDetails (IExceptionHandler output). */
export interface ProblemDetails {
  type?: string;
  title: string;
  status: number;
  detail?: string;
  errorCode?: string; // backend WC-NNNN extension member
  errors?: Record<string, string[]>; // field -> messages (validation)
}

/** Normalises an Axios error into a typed ProblemDetails. */
export function parseProblemDetails(error: unknown): ProblemDetails {
  const response = (error as { response?: { status?: number; data?: unknown } })?.response;
  const data = response?.data;
  if (data && typeof data === "object" && "status" in data) {
    return data as ProblemDetails;
  }
  return {
    title: (error as { message?: string })?.message ?? "Network error",
    status: response?.status ?? 0,
  };
}
