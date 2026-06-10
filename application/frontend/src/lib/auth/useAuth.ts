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

/** Where to send a user after login: admins to the admin console, everyone else to their dashboard. */
export function landingPathForRoles(roles: string[]): string {
  return roles.includes("Admin") ? "/admin" : "/dashboard";
}

/** Landing path for the currently authenticated user (reads the auth store inside lib/auth). */
export function landingPath(): string {
  return landingPathForRoles(authStore.getUser()?.roles ?? []);
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
