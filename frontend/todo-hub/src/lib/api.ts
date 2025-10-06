import axios from "axios";
const API_URL = process.env.NEXT_PUBLIC_API_URL || "https://localhost:7098";


const api = axios.create({
  baseURL: API_URL,
  headers: {
    "Content-Type": "application/json",
  },
  withCredentials: true, // with cookies
});

// interceptor 
api.interceptors.request.use((config) => {
  const token = document.cookie
      .split("; ")
      .find(row => row.startsWith("accessToken="))
      ?.split("=")[1] || null;
  if (token) {
    config.headers!["Authorization"] = `Bearer ${token}`;
  }
  return config;
});

// interceptor for refresh 401
api.interceptors.response.use(
  (res) => res,
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      try {
        const res = await refreshToken();
        document.cookie = `accessToken=${res.value.token}; path=/;`;
        originalRequest.headers["Authorization"] = `Bearer ${res.value.token}`;
        return api.request(originalRequest); // retry original request
      } catch (err) {
        return Promise.reject(err);
      }
    }
    return Promise.reject(error);
  }
);

// interface for login response
interface LoginResponse {
  success: boolean,
  error: string | null,
  value: {
    token: string;
    expiredAt: string;
  };
  message: string | null
}

// Login function
export const login = async (data: { email: string; password: string }): Promise<LoginResponse> => {
  const res = await api.post(`${API_URL}/auth/login`, data, { withCredentials: true });

  if (res.status === 200 && res.data.value?.token) {
    document.cookie = `accessToken=${res.data.value.token}; path=/;`;
    return res.data;
  } else {
    throw new Error(res.data.error || "Login failed");
  }
};

// register function
export const register = async (data: { name: string; email: string, password: string, confirmPassword: string }) => 
{
  const res = await api.post(`${API_URL}/auth/register`, data, { withCredentials: true });
  if (res.status !== 201) throw new Error(res.data.error || "Registration failed");
  return res.data;
}

// logOut function
export const logOut = async () => {
    const res = await api.post(`${API_URL}/auth/logout`, {}, { withCredentials: true });
    if (res.status !== 200) throw new Error(res.data.error || "Logout failed");
    api.defaults.headers.Authorization = "";
    return res.data;
}

// Fetch user profile
export const getMe = async () => {
  const res = await api.get(`${API_URL}/profile`,{ withCredentials: true });
  if (res.status !== 200)
  {
    console.log(res);
    throw new Error("Error fetching profile");
  }
    
  return res.data;
}


// refresh token function
export async function refreshToken() {
  const res = await api.post(`${API_URL}/auth/refresh`, {}, { withCredentials: true });
  if (res.status !== 200) throw new Error("Error refreshing token");
  return res.data;
}

export default api;