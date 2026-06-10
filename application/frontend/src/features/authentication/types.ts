/** Mirrors the backend Login request/response (slice 01-auth). Adjust to the built DTO when available. */
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
}
