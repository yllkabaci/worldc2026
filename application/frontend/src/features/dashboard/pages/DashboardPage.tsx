import { useAuth } from "../../../lib/auth/useAuth";

export function DashboardPage() {
  const { user, logout } = useAuth();
  return (
    <main className="container">
      <h1>Dashboard</h1>
      <p>Signed in as {user?.email ?? "unknown"}</p>
      <button className="btn" onClick={logout}>
        Log out
      </button>
    </main>
  );
}
