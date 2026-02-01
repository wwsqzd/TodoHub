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
        document.cookie = `accessToken=${res.value.token}; path=/;`;
        originalRequest.headers["Authorization"] = `Bearer ${res.value.token}`;
        return api.request(originalRequest); // retry
      } catch (err) {
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
      if (err.response.status === 504) {
        throw new Error("The server is taking too long to respond. Please try again later.");
      }
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
export const register = async (data: { name: string; email: string; password: string; confirmPassword: string }) => {
  try {
    const res = await api.post(`${API_URL}/auth/register`, data, { withCredentials: true });
    if (res.status !== 201) throw new Error(res.data.error || "Registration failed");
    return res.data;
  } catch (err: unknown) {
    if (axios.isAxiosError(err) && err.response?.status === 504) {
      throw new Error("The server is taking too long to respond. Please try again later.");
    } else if (axios.isAxiosError(err)) {
      const message = err.response?.data?.message || "Error registering";
      throw new Error(message);
    } else {
      throw new Error("Unknown error");
    }
  }
};

// logOut function
export const logOut = async () => {
  try {
    const res = await api.post(`${API_URL}/auth/logout`, {}, { withCredentials: true });
    if (res.status !== 200) throw new Error(res.data.error || "Logout failed");
    api.defaults.headers.Authorization = "";
    return res.data;
  } catch (err: unknown) {
    if (axios.isAxiosError(err) && err.response?.status === 504) {
      throw new Error("The server is taking too long to respond. Please try again later.");
    } else if (axios.isAxiosError(err)) {
      const message = err.response?.data?.message || "Error logging out";
      throw new Error(message);
    } else {
      throw new Error("Unknown error");
    }
  }
};

// Fetch user profile
export const getMe = async () => {
  try {
    const res = await api.get(`${API_URL}/profile`, { withCredentials: true });
    if (res.status !== 200) {
      console.log(res);
      throw new Error("Error fetching profile");
    }
    return res.data;
  } catch (err: unknown) {
    if (axios.isAxiosError(err) && err.response?.status === 504) {
      throw new Error("The server is taking too long to respond. Please try again later.");
    } else if (axios.isAxiosError(err)) {
      const message = err.response?.data?.message || "Error fetching profile";
      throw new Error(message);
    } else {
      throw new Error("Unknown error");
    }
  }
};


// refresh token function
export const refreshToken = async () => {
  try {
    const res = await api.post(`${API_URL}/auth/refresh`, {}, { withCredentials: true });
    if (res.status !== 200) throw new Error("Error refreshing token");
    return res.data;
  } catch (err: unknown) {
    if (axios.isAxiosError(err) && err.response?.status === 504) {
      throw new Error("The server is taking too long to respond. Please try again later.");
    } else if (axios.isAxiosError(err)) {
      const message = err.response?.data?.message || "Error refreshing token";
      throw new Error(message);
    } else {
      throw new Error("Unknown error");
    }
  }
};

// fetch is user admin
export const fetchIsAdmin = async () => {
  try {
    const res = await api.get(`${API_URL}/profile/role`, { withCredentials: true });
    if (res.status !== 200) throw new Error("Error checking admin status");
    return res.data;
  } catch (err: unknown) {
    if (axios.isAxiosError(err) && err.response?.status === 504) {
      throw new Error("The server is taking too long to respond. Please try again later.");
    } else if (axios.isAxiosError(err)) {
      const message = err.response?.data?.message || "Error checking admin status";
      throw new Error(message);
    } else {
      throw new Error("Unknown error");
    }
  }
};

// fetch users (admin)
export const GetUsers = async () => {
  try {
    const res = await api.get(`${API_URL}/users`, { withCredentials: true });
    console.log(res);
    if (res.status !== 200) throw new Error("Error fetching users");
    return res.data;
  } catch (err: unknown) {
    if (axios.isAxiosError(err) && err.response?.status === 504) {
      throw new Error("The server is taking too long to respond. Please try again later.");
    } else if (axios.isAxiosError(err)) {
      const message = err.response?.data?.message || "Error fetching users";
      throw new Error(message);
    } else {
      throw new Error("Unknown error");
    }
  }
};

// delete user (admin)
export const DeleteUser = async (id: string) => {
  try {
    const res = await api.delete(`${API_URL}/user/delete/${id}`, { withCredentials: true });
    if (res.status !== 200) throw new Error("Error deleting user");
    return res.data;
  } catch (err: unknown) {
    if (axios.isAxiosError(err) && err.response?.status === 504) {
      throw new Error("The server is taking too long to respond. Please try again later.");
    } else if (axios.isAxiosError(err)) {
      const message = err.response?.data?.message || "Error deleting user";
      throw new Error(message);
    } else {
      throw new Error("Unknown error");
    }
  }
};

export const ChangeUserlanguage = async (language: string) => {
  try {
    const res = await api.patch(`${API_URL}/profile/language`, { Language: language }, { withCredentials: true });
    if (res.status !== 200) throw new Error("Error changing language");
    return res.data;
  } catch (err: unknown) {
    if (axios.isAxiosError(err) && err.response?.status === 504) {
      throw new Error("The server is taking too long to respond. Please try again later.");
    } else if (axios.isAxiosError(err)) {
      const message = err.response?.data?.message || "Error changing language";
      throw new Error(message);
    } else {
      throw new Error("Unknown error");
    }
  }
};





// todos


// fetch todos
export const getUserTodos = async (params: URLSearchParams) => {
  try {
    const res = await api.get(`${API_URL}/todos?${params.toString()}`, { withCredentials: true });
    console.log(res);
    return res.data;
  } catch (err: unknown) {
    if (axios.isAxiosError(err) && err.response?.status === 504) {
      throw new Error("The server is taking too long to respond. Please try again later.");
    } else if (axios.isAxiosError(err)) {
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
    if (axios.isAxiosError(err) && err.response?.status === 504) {
      throw new Error("The server is taking too long to respond. Please try again later.");
    }
    else if (axios.isAxiosError(err)) {
      const message = err.response?.data?.message || "Error creating todo";
      throw new Error(message);
    } else {
      throw new Error("Unknown error");
    }
  }
};

// get todo
export const getTodo = async (id: string) => {
  try {
    const res = await api.get(`${API_URL}/todos/${id}`, { withCredentials: true });
    if (res.status !== 200) throw new Error("Error fetching Todo");
    return res.data;
  } catch (err: unknown) {
    if (axios.isAxiosError(err) && err.response?.status === 504) {
      throw new Error("The server is taking too long to respond. Please try again later.");
    } else if (axios.isAxiosError(err)) {
      const message = err.response?.data?.message || "Error fetching Todo";
      throw new Error(message);
    } else {
      throw new Error("Unknown error");
    }
  }
};

// delete todo
export const deleteTodo = async (id: string) => {
  try {
    const res = await api.delete(`${API_URL}/todos/${id}`, { withCredentials: true });
    if (res.status !== 200) throw new Error("Error deleting Todo");
    return res.data;
  } catch (err: unknown) {
    if (axios.isAxiosError(err) && err.response?.status === 504) {
      throw new Error("The server is taking too long to respond. Please try again later.");
    } else if (axios.isAxiosError(err)) {
      const message = err.response?.data?.message || "Error deleting Todo";
      throw new Error(message);
    } else {
      throw new Error("Unknown error");
    }
  }
};

// rewrite todo
export const modifyTodo = async (id: string, data: { title: string; description: string; isCompleted: boolean }) => {
  try {
    const res = await api.patch(`${API_URL}/todos/${id}`, data, { withCredentials: true });
    if (res.status !== 200) throw new Error("Error modify todo");
    return res.data;
  } catch (err: unknown) {
    if (axios.isAxiosError(err) && err.response?.status === 504) {
      throw new Error("The server is taking too long to respond. Please try again later.");
    } else if (axios.isAxiosError(err)) {
      const message = err.response?.data?.message || "Error modify todo";
      throw new Error(message);
    } else {
      throw new Error("Unknown error");
    }
  }
};

// ---- Search Todos ----


export const searchTodos = async (q: string) => {
  try {
    const res = await api.get(`${API_URL}/search?q=${encodeURIComponent(q)}`, {
      withCredentials: true,
    });
    if (res.status !== 200) throw new Error("Error searching todos");
    return res.data;
  } catch (err: unknown) {
    if (axios.isAxiosError(err) && err.response?.status === 504) {
      throw new Error("The server is taking too long to respond. Please try again later.");
    } else if (axios.isAxiosError(err)) {
      const message = err.response?.data?.message || "Error searching todos";
      throw new Error(message);
    } else {
      throw new Error("Unknown error");
    }
  }
};



export default api;