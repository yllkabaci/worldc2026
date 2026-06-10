import { useMutation } from "@tanstack/react-query";
import { apiClient } from "../../../lib/api/client";
import { unwrap, type ApiResponse } from "../../../lib/api/apiResponse";
import { logger } from "../../../lib/logging/logger";
import type { RegisterRequest, RegisterResponse } from "../types";

/**
 * Registers a new account. The backend returns no token, so the user is NOT logged in here —
 * the caller redirects to /login afterwards (see RegisterPage).
 */
export function useRegister() {
  return useMutation({
    mutationFn: (body: RegisterRequest) =>
      apiClient.post<ApiResponse<RegisterResponse>>("/api/auth/register", body).then(unwrap),
    onSuccess: () => {
      logger.event("register_succeeded");
    },
  });
}
