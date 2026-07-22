import axios from "axios";
import { getToken, clearToken } from "../utils/tokenStorage";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

api.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  // El default "Content-Type: application/json" de la instancia pisa el
  // boundary multipart que necesita un FormData (usado en las subidas de
  // fotos) — hay que sacarlo para que el navegador ponga el suyo con el
  // boundary correcto.
  if (config.data instanceof FormData) {
    if (typeof config.headers?.delete === "function") {
      config.headers.delete("Content-Type");
    } else {
      delete config.headers["Content-Type"];
    }
  }

  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      clearToken();
    }
    return Promise.reject(error);
  }
);

export default api;
