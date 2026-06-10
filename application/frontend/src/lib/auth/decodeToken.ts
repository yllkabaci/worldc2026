export interface AuthUser {
  id: string;
  email: string;
  roles: string[];
}

const ROLE_URI = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

/** Decodes JWT claims for UX role-gating only. The backend still enforces every policy. */
export function decodeToken(token: string): AuthUser | null {
  try {
    const segment = token.split(".")[1];
    if (!segment) return null;
    const json = atob(segment.replace(/-/g, "+").replace(/_/g, "/"));
    const payload = JSON.parse(json) as Record<string, unknown>;
    const rawRole = payload.role ?? payload[ROLE_URI];
    const roles = Array.isArray(rawRole) ? (rawRole as string[]) : rawRole ? [String(rawRole)] : [];
    return {
      id: String(payload.sub ?? ""),
      email: String(payload.email ?? ""),
      roles,
    };
  } catch {
    return null;
  }
}
