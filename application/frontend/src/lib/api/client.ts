import axios from "axios";
import { authStore } from "../auth/authStore";
import { parseProblemDetails } from "./problemDetails";

/** The single Axios instance. Feature api/ functions import this; never axios.create() or fetch elsewhere. */
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  timeout: 15_000,
});

// 1. attach the in-memory JWT bearer to every request
apiClient.interceptors.request.use((config) => {
  const token = authStore.getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// 2. handle 401 globally (no refresh in the MVP) and normalise errors to ProblemDetails
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if ((error as { response?: { status?: number } })?.response?.status === 401) {
      authStore.clearAndRedirect();
    }
    return Promise.reject(parseProblemDetails(error));
  },
);
