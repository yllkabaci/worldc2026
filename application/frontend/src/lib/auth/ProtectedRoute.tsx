import type { ReactNode } from "react";
import { Navigate } from "react-router-dom";
import { useAuth, type AppRole } from "./useAuth";
import { ForbiddenView } from "../../components/ForbiddenView";

interface Props {
  role: AppRole;
  children: ReactNode;
}

/** Client-side route gate mirroring the backend authorization policy. The server still enforces it. */
export function ProtectedRoute({ role, children }: Props) {
  const { isAuthenticated, hasRole } = useAuth();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (!hasRole(role)) return <ForbiddenView />;
  return <>{children}</>;
}
