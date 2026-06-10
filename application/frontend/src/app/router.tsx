import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { HomePage } from "../features/home/pages/HomePage";
import { LoginPage } from "../features/authentication/pages/LoginPage";
import { RegisterPage } from "../features/authentication/pages/RegisterPage";
import { DashboardPage } from "../features/dashboard/pages/DashboardPage";
import { AdminDashboardPage } from "../features/admin/pages/AdminDashboardPage";
import { ProtectedRoute } from "../lib/auth/ProtectedRoute";
import { NotFound } from "../components/NotFound";

const router = createBrowserRouter([
  { path: "/", element: <HomePage /> },
  { path: "/login", element: <LoginPage /> },
  { path: "/register", element: <RegisterPage /> },
  {
    path: "/dashboard",
    element: (
      <ProtectedRoute role="User">
        <DashboardPage />
      </ProtectedRoute>
    ),
  },
  {
    path: "/admin",
    element: (
      <ProtectedRoute role="Admin">
        <AdminDashboardPage />
      </ProtectedRoute>
    ),
  },
  { path: "*", element: <NotFound /> },
]);

export function AppRouter() {
  return <RouterProvider router={router} />;
}
