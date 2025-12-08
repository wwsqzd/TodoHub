"use client";

import { useRef, useState } from "react";
import { login, loginWithGitHub, loginWithGoogle } from "@/lib/api";
import Link from "next/link";
import { useAuth } from "@/context/AuthContext";
import axios from "axios";
import gsap from "gsap";
import { FcGoogle } from "react-icons/fc";
import { FaGithub } from "react-icons/fa6";
import { useLanguage } from "@/context/LanguageContext";
import { translations } from "@/lib/dictionary";

export default function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const { setAccessToken } = useAuth();

  const loginRef = useRef<HTMLDivElement>(null);

  const { language } = useLanguage();
  const t = translations[language];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      const response = await login({ email, password });
      setAccessToken(response.value.token);
      setEmail("");
      setPassword("");
      window.location.href = "/profile"; // Redirect to home page
    } catch (err: unknown) {
      if (err) {
        gsap.fromTo(
          loginRef.current,
          { x: -10 },
          {
            x: 0,
            duration: 0.5,
            ease: "power1.inOut",
            keyframes: [
              { x: -8 },
              { x: 8 },
              { x: -5 },
              { x: 5 },
              { x: -3 },
              { x: 3 },
              { x: 0 },
            ],
          }
        );
      }
      if (axios.isAxiosError(err)) {
        if (err.response?.status === 401) {
          setError("Invalid email or password");
          return;
        }
        if (err.response?.status === 500) {
          setError("Server error. Please try again later.");
          return;
        }
        if (err.response?.status === 429) {
          setError("Too many requests, calm down");
          return;
        }
      }
      if (err instanceof Error)
        setError(err.message || "An unexpected error occurred");
    } finally {
      setLoading(false);
    }
  };

  const handleLoginWithGoogle = async () => {
    setLoading(true);
    setError(null);
    try {
      await loginWithGoogle();
    } catch (err: unknown) {
      console.error("Google login failed:", err);
    } finally {
      setLoading(false);
    }
  };

  const handleLoginWithGitHub = async () => {
    setLoading(true);
    setError(null);
    try {
      await loginWithGitHub();
    } catch (err: unknown) {
      console.error("GitHub login failed:", err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div ref={loginRef} className="flex flex-col items-center">
      <h1 className="text-2xl font-bold">Login</h1>
      <div className="w-40 m-3 flex justify-center gap-3  ">
        <div className="p-1 cursor-pointer" onClick={handleLoginWithGoogle}>
          <FcGoogle size={32} />
        </div>
        <div className="p-1 cursor-pointer" onClick={handleLoginWithGitHub}>
          <FaGithub size={30} />
        </div>
      </div>
      <form
        onSubmit={handleSubmit}
        className="flex flex-col gap-4 w-full max-w-xs"
      >
        <input
          type="email"
          placeholder={t.email}
          autoComplete="email"
          required
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          className="border rounded px-3 py-2"
        />
        <input
          type="password"
          placeholder={t.password}
          autoComplete="current-password"
          required
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          className="border rounded px-3 py-2"
        />
        <div className="text-sm text-gray-600 mb-4 text-center">
          <p>
            {t.dontHaveAccount}{" "}
            <Link
              href="/auth/register"
              className="text-blue-600 hover:underline"
            >
              {t.signUp}
            </Link>
          </p>
        </div>
        <button
          type="submit"
          className="bg-blue-600 text-white rounded py-2 font-semibold cursor-pointer"
        >
          {loading ? "Loading..." : <>{t.login}</>}
        </button>
        <div className="text-sm text-gray-600 text-center">
          {error && <p className="text-red-500 mb-2">{error}</p>}
        </div>
      </form>
    </div>
  );
}
