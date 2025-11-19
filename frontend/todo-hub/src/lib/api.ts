import axios from "axios";
const API_URL = process.env.NEXT_PUBLIC_API_URL || "https://localhost:7098/api";



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
    // if 401 haven't tried again yet
    if (
      error.response?.status === 401 && 
      !originalRequest._retry && 
      !originalRequest.url.includes("/auth/login") &&
      !originalRequest.url.includes("/auth/register") &&
      !originalRequest.url.includes("/auth/refresh")
    ) {
      originalRequest._retry = true;
      try {
        const res = await refreshToken();
        console.log(res);
        document.cookie = `accessToken=${res.value.token}; path=/;`;
        originalRequest.headers["Authorization"] = `Bearer ${res.value.token}`;
        return api.request(originalRequest); // retry
      } catch (err) {
        console.log(err);
        document.cookie = "accessToken=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT";
        window.location.href = "/auth/login"; 
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
export const login = async (
  data: { email: string; password: string }
): Promise<LoginResponse> => {
  try {
    const res = await api.post(`${API_URL}/auth/login`, data, { withCredentials: true });
    
    if (res.status === 200 && res.data.value?.token) {
      document.cookie = `accessToken=${res.data.value.token}; path=/;`;
      return res.data; 
    }

    throw new Error(res.data.error || "Login failed");
  } catch (err: unknown) {
    if (axios.isAxiosError(err) && err.response) {
      if (err.response.status === 429) {
        throw err;
      }
      throw new Error(err.response.data?.error || `Request failed with status ${err.response.status}`);
    }

    if (err instanceof Error) throw err;
    throw new Error("Unknown error");
  }
};

// login with google function
export const loginWithGoogle = async () => {
  window.location.href = `${API_URL}/auth/login/google`;
};

// login with gitHub function
export const loginWithGitHub = async () => {
  window.location.href = `${API_URL}/auth/login/github`;
}

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
export const refreshToken = async () => {
  const res = await api.post(`${API_URL}/auth/refresh`, {}, { withCredentials: true });
  if (res.status !== 200) throw new Error("Error refreshing token");
  return res.data;
}

// fetch is user admin
export const fetchIsAdmin = async () => {
  const res = await api.get(`${API_URL}/profile/role`, { withCredentials: true });
  if (res.status !== 200) throw new Error("Error checking admin status");
  return res.data;
}

// fetch users (admin)
export const GetUsers = async () => {
  const res = await api.get(`${API_URL}/users`, {withCredentials: true});
  if (res.status !== 200) throw new Error("Error fetching users")
  return res.data;
}

// delete user (admin)
export const DeleteUser = async (id: string) => {
  const res = await api.delete(`${API_URL}/user/delete/${id}`, {withCredentials: true});
  if (res.status !== 200) throw new Error("Error deleting user");
  return res.data;
} 






// todos


// fetch todos
export const getUserTodos = async (params: URLSearchParams) => {
  try {
    const res = await api.get(`${API_URL}/todos?${params.toString()}`, { withCredentials: true });
    return res.data;
  } catch (err: unknown)
  {
    if (axios.isAxiosError(err)) {
      const message = err.response?.data || "Error fetching todos";
      throw new Error(message);
    } else {
      throw new Error("Unknown error");
    }
  }
};

// create todo
export const createTodo = async (data: { title: string; description: string }) => {
  try {
    const res = await api.post(`${API_URL}/todos/create`, data, { withCredentials: true });
    return res.data;
  } catch (err: unknown) {
    if (axios.isAxiosError(err)) {
      const message = err.response?.data?.message || "Error creating todo";
      throw new Error(message);
    } else {
      throw new Error("Unknown error");
    }
  }
};

// get todo
export const getTodo = async (id: string) => {
  const res = await api.get(`${API_URL}/todos/${id}`, {withCredentials: true});
  if (res.status !== 200) throw new Error("Error fetching Todo")
  return res.data;
}

// delete todo
export const deleteTodo = async (id:string) => {
  const res = await api.delete(`${API_URL}/todos/${id}`, {withCredentials: true});
  if (res.status !== 200) throw new Error("Error deleting Todo")
  return res.data;
}

// rewrite todo
export const modifyTodo = async (id: string, data: {title: string, description: string, isCompleted: boolean}) => {
  const res = await api.patch(`${API_URL}/todos/${id}`, data, {withCredentials: true});
  if (res.status !== 200) throw new Error("Error modify todo")
  return res.data;
}

export default api;