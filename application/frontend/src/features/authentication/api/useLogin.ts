import { useMutation } from "@tanstack/react-query";
import { apiClient } from "../../../lib/api/client";
import { unwrap, type ApiResponse } from "../../../lib/api/apiResponse";
import { authStore } from "../../../lib/auth/authStore";
import { logger } from "../../../lib/logging/logger";
import type { LoginRequest, LoginResponse } from "../types";

export function useLogin() {
  return useMutation({
    mutationFn: (body: LoginRequest) =>
      apiClient.post<ApiResponse<LoginResponse>>("/api/auth/login", body).then(unwrap),
    onSuccess: (data) => {
      authStore.setSession(data.token);
      logger.event("login_succeeded");
    },
  });
}
