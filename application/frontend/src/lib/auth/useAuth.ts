import { useCallback, useSyncExternalStore } from "react";
import { authStore } from "./authStore";

export type AppRole = "User" | "Admin" | "SuperAdmin";

const RANK: Record<AppRole, number> = { User: 1, Admin: 2, SuperAdmin: 3 };

/** Role hierarchy mirroring the backend policies: Admin satisfies User, SuperAdmin satisfies Admin. */
export function hasRoleHierarchy(roles: string[], required: AppRole): boolean {
  const max = roles.reduce((acc, r) => Math.max(acc, RANK[r as AppRole] ?? 0), 0);
  return max >= RANK[required];
}

export function useAuth() {
  const token = useSyncExternalStore(authStore.subscribe, authStore.getToken, () => null);
  const user = authStore.getUser();
  const roles = user?.roles ?? [];

  const hasRole = useCallback((role: AppRole) => hasRoleHierarchy(roles, role), [roles]);
  const logout = useCallback(() => authStore.clearAndRedirect(), []);

  return {
    isAuthenticated: Boolean(token),
    user,
    roles,
    hasRole,
    logout,
  };
}
