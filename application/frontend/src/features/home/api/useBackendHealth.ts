import { useQuery } from "@tanstack/react-query";
import { apiClient } from "../../../lib/api/client";
import { queryKeys } from "../../../lib/api/queryKeys";

/** Hits the backend /healthz endpoint to prove connectivity (and CORS) end to end. */
export function useBackendHealth() {
  return useQuery({
    queryKey: queryKeys.health(),
    queryFn: async ({ signal }) => {
      const res = await apiClient.get<string>("/healthz", { signal, responseType: "text" });
      return res.data;
    },
    staleTime: 10_000,
    retry: false,
  });
}
