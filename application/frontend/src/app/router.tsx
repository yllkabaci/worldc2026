import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { HomePage } from "../features/home/pages/HomePage";
import { LoginPage } from "../features/authentication/pages/LoginPage";
import { DashboardPage } from "../features/dashboard/pages/DashboardPage";
import { ProtectedRoute } from "../lib/auth/ProtectedRoute";
import { NotFound } from "../components/NotFound";

const router = createBrowserRouter([
  { path: "/", element: <HomePage /> },
  { path: "/login", element: <LoginPage /> },
  {
    path: "/dashboard",
    element: (
      <ProtectedRoute role="User">
        <DashboardPage />
      </ProtectedRoute>
    ),
  },
  { path: "*", element: <NotFound /> },
]);

export function AppRouter() {
  return <RouterProvider router={router} />;
}
