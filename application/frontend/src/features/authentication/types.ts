/** Auth DTOs — mirror the backend Authentication slice 1:1. */

/** POST /api/auth/login body. */
export interface LoginRequest {
  email: string;
  password: string;
}

/** POST /api/auth/login → data. */
export interface LoginResponse {
  token: string;
}

/** POST /api/auth/register body. */
export interface RegisterRequest {
  email: string;
  password: string;
}

/** POST /api/auth/register → data (no token; the user logs in afterwards). */
export interface RegisterResponse {
  userId: string;
  email: string;
}
