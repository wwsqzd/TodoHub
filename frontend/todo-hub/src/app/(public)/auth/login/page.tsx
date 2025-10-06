"use client";

import { useState } from "react";
import { login } from "@/lib/api";
import Link from "next/link";
import { useAuth } from "@/context/AuthContext";

export default function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const { setAccessToken } = useAuth();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
        const response = await login({ email, password });
        setAccessToken(response.value.token);
        window.location.href = "/profile"; // Redirect to home page
    } catch (err: unknown) {
        if (err instanceof Error)
        setError(err.message || "An unexpected error occurred");
    } finally {
        setLoading(false);
    }
  };

  return (
    <div className="flex flex-col items-center">
      <h1 className="mb-6 text-2xl font-bold">Login</h1>
      <form onSubmit={handleSubmit} className="flex flex-col gap-4 w-full max-w-xs">
        <input
          type="email"
          placeholder="Email"
          autoComplete="email"
          required
          value={email}
          onChange={e => setEmail(e.target.value)}
          className="border rounded px-3 py-2"
        />
        <input
          type="password"
          placeholder="Password"
          autoComplete="current-password"
          required
          value={password}
          onChange={e => setPassword(e.target.value)}
          className="border rounded px-3 py-2"
        />
        <div className="text-sm text-gray-600 mb-4 text-center">
            <p>
            Don&apos;t have an account?{" "}
            <Link href="/auth/register" className="text-blue-600 hover:underline">
                Sign up
            </Link>
            </p>
        </div>
        <button type="submit" className="bg-blue-600 text-white rounded py-2 font-semibold cursor-pointer">
          {loading ? "Loading..." : "Login"}
        </button>
        <div className="text-sm text-gray-600">
            {error && <p className="text-red-500 mb-2">{error}</p>}
        </div>
      </form>
    </div>
  );
}