import { useCallback, useSyncExternalStore } from "react";
import { authStore } from "./authStore";

export type AppRole = "User" | "Admin";

/**
 * Two roles only (mirrors the backend `IsAdmin` flag): every authenticated account satisfies
 * "User"; accounts whose token carries the "Admin" role also satisfy "Admin".
 */
export function roleMatches(roles: string[], required: AppRole): boolean {
  if (required === "Admin") return roles.includes("Admin");
  return roles.length > 0; // any role ⇒ authenticated
}

export function useAuth() {
  const token = useSyncExternalStore(authStore.subscribe, authStore.getToken, () => null);
  const user = authStore.getUser();
  const roles = user?.roles ?? [];
  const isAuthenticated = Boolean(token);
  const isAdmin = roles.includes("Admin");

  const hasRole = useCallback(
    (role: AppRole) => (role === "Admin" ? isAdmin : isAuthenticated),
    [isAdmin, isAuthenticated],
  );
  const logout = useCallback(() => authStore.clearAndRedirect(), []);

  return { isAuthenticated, user, roles, isAdmin, hasRole, logout };
}
