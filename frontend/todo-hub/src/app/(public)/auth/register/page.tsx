"use client";

import { useState } from "react";
import Link from "next/link";
import { register } from "@/lib/api";

export default function RegisterPage() {
  const [email, setEmail] = useState("");
  const [name, setName] = useState("");
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (password !== confirm) {
      setError("Passwords do not match");
      return;
    }

    setLoading(true);
    try {
      await register({ email, name, password, confirmPassword: confirm });
      window.location.href = "/auth/login";
    } catch (err: unknown) {
      if (err instanceof Error)
      {
        setError(err.message || "An unexpected error occurred");
        console.log(err.message);
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex flex-col items-center">
      <h1 className="mb-6 text-2xl font-bold">Register</h1>
      <form onSubmit={handleSubmit} className="flex flex-col gap-4 w-full max-w-xs">
        <input
          type="text"
          placeholder="Name"
          autoComplete="name"
          required
          value={name}
          onChange={e => setName(e.target.value)}
          className="border rounded px-3 py-2"
        />
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
          autoComplete="new-password"
          required
          value={password}
          onChange={e => setPassword(e.target.value)}
          className="border rounded px-3 py-2"
        />
        <input
          type="password"
          placeholder="Confirm password"
          autoComplete="new-password"
          required
          value={confirm}
          onChange={e => setConfirm(e.target.value)}
          className="border rounded px-3 py-2"
        />
        <div className="text-sm text-gray-600 mb-4 text-center">
          <p>
            Already have an account?{" "}
            <Link href="/auth/login" className="text-blue-600 hover:underline">
              Login
            </Link>
          </p>
        </div>
        <button type="submit" className="bg-blue-600 text-white rounded py-2 font-semibold cursor-pointer">
          {loading ? "Loading..." : "Register"}
        </button>
        <div className="text-sm text-gray-600">
          {error && <p className="text-red-500 mb-2">{error}</p>}
        </div>
      </form>
    </div>
  );
}