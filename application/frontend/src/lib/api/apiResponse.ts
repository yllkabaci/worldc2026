import type { AxiosResponse } from "axios";

/** Mirrors the backend ApiResponse<T> success envelope. */
export interface ApiResponse<T> {
  success: boolean;
  data: T;
}

/** Unwraps an Axios response whose body is ApiResponse<T> into the inner payload T. */
export const unwrap = <T>(res: AxiosResponse<ApiResponse<T>>): T => res.data.data;
