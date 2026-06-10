import { decodeToken, type AuthUser } from "./decodeToken";

type Listener = () => void;

let token: string | null = null;
let user: AuthUser | null = null;
const listeners = new Set<Listener>();
const emit = () => listeners.forEach((l) => l());

/**
 * In-memory auth state (MVP: no refresh, never persisted to web storage).
 * Used by the Axios interceptor and exposed to React via useAuth().
 */
export const authStore = {
  getToken: (): string | null => token,
  getUser: (): AuthUser | null => user,
  setSession(newToken: string): void {
    token = newToken;
    user = decodeToken(newToken);
    emit();
  },
  clear(): void {
    token = null;
    user = null;
    emit();
  },
  clearAndRedirect(to = "/login"): void {
    this.clear();
    if (typeof window !== "undefined") {
      window.location.assign(to);
    }
  },
  subscribe(listener: Listener): () => void {
    listeners.add(listener);
    return () => listeners.delete(listener);
  },
};
