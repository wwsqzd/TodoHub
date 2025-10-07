"use client";
import { createContext, useContext, useState, ReactNode, useEffect } from "react";
import { fetchIsAdmin } from "@/lib/api";

type AuthContextType = {
  accessToken: string | null;
  setAccessToken: (token: string | null) => void;
  isAdmin?: boolean;
  setIsAdmin?: (isAdmin: boolean) => void;
};

const AuthContext = createContext<AuthContextType>({
  accessToken: null,
  setAccessToken: () => {},
  isAdmin: false,
  setIsAdmin: () => {},
});

export function AuthProvider({ children }: { children: ReactNode }) {
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [isAdmin, setIsAdmin] = useState<boolean>(false);

  useEffect(() => {
  const checkToken = async () => {
    const token = document.cookie
      .split("; ")
      .find(row => row.startsWith("accessToken="))
      ?.split("=")[1] || null;
      if (token)
      {
        await fetchIsAdmin().then(res => {
            setIsAdmin(res.value);
        });
      }
    setAccessToken(token);
  };
  checkToken(); 
  }, []);

  return (
    <AuthContext.Provider value={{ accessToken, setAccessToken, isAdmin }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);