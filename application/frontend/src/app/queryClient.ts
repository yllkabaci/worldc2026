import { QueryClient } from "@tanstack/react-query";
import type { ProblemDetails } from "../lib/api/problemDetails";

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: (failureCount, error) => {
        const status = (error as ProblemDetails)?.status ?? 0;
        if (status >= 400 && status < 500) return false; // don't retry client errors
        return failureCount < 2;
      },
    },
  },
});
