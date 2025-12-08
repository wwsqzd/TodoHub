"use client";

import { useRef, useState } from "react";
import Link from "next/link";
import { register } from "@/lib/api";
import axios from "axios";
import zxcvbn from "zxcvbn";
import { BsEye } from "react-icons/bs";
import { BsEyeSlash } from "react-icons/bs";
import gsap from "gsap";
import { useLanguage } from "@/context/LanguageContext";
import { translations } from "@/lib/dictionary";

export default function RegisterPage() {
  const [email, setEmail] = useState("");
  const [name, setName] = useState("");
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);

  const colors = [
    "bg-red-500",
    "bg-orange-500",
    "bg-yellow-500",
    "bg-lime-500",
    "bg-green-600",
  ];

  const { language } = useLanguage();
  const t = translations[language];

  const labels = ["Weak", "Fair", "Good", "Strong", "Very Strong"];
  const strength = zxcvbn(password).score;

  const registerRef = useRef<HTMLDivElement>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (password !== confirm) {
      setError("Passwords do not match");
      return;
    }

    if (strength < 3) {
      setError("Password is too weak");
      return;
    }

    setLoading(true);
    try {
      await register({ email, name, password, confirmPassword: confirm });
      window.location.href = "/auth/login";
    } catch (err: unknown) {
      if (err) {
        gsap.fromTo(
          registerRef.current,
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
        if (err.response?.status === 400) {
          setError("Invalid input data. Please check your entries.");
          return;
        }
        if (err.response?.status === 500) {
          setError("Server error. Please try again later.");
          return;
        }
        if (err.response?.status === 409) {
          setError("Account with this email already exists.");
          return;
        }
      }
      if (err instanceof Error) {
        setError(err.message || "An unexpected error occurred");
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div ref={registerRef} className="flex flex-col items-center">
      <h1 className="mb-6 text-2xl font-bold">{t.signUp}</h1>
      <form
        onSubmit={handleSubmit}
        className="flex flex-col gap-4 w-full max-w-xs"
      >
        <input
          type="text"
          placeholder={t.name}
          autoComplete="name"
          required
          value={name}
          onChange={(e) => setName(e.target.value)}
          className="border rounded px-3 py-2"
        />
        <input
          type="email"
          placeholder={t.email}
          autoComplete="email"
          required
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          className="border rounded px-3 py-2"
        />
        <div className="relative">
          <input
            type={showPassword ? "text" : "password"}
            placeholder={t.password}
            autoComplete="new-password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="border rounded px-3 py-2 w-full pr-10"
          />
          <button
            type="button"
            className="absolute right-2 top-1/2 -translate-y-1/2 text-xl text-gray-500 cursor-pointer"
            onClick={() => setShowPassword((prev) => !prev)}
            tabIndex={-1}
          >
            {showPassword ? <BsEyeSlash /> : <BsEye />}
          </button>
        </div>
        <div className="relative">
          <input
            type={showConfirm ? "text" : "password"}
            placeholder={t.confirmPassword}
            autoComplete="new-password"
            required
            value={confirm}
            onChange={(e) => setConfirm(e.target.value)}
            className="border rounded px-3 py-2 w-full pr-10"
          />
          <button
            type="button"
            className="absolute right-2 top-1/2 -translate-y-1/2 text-xl text-gray-500 cursor-pointer"
            onClick={() => setShowConfirm((prev) => !prev)}
            tabIndex={-1}
          >
            {showConfirm ? <BsEyeSlash /> : <BsEye />}
          </button>
        </div>
        <div>
          {password && (
            <>
              <div className="h-2 w-full bg-gray-200 rounded mt-2">
                <div
                  className={`h-2 rounded transition-all ${colors[strength]}`}
                  style={{ width: `${(strength + 1) * 20}%` }}
                />
              </div>
              <p className="text-sm mt-1 text-gray-600">{labels[strength]}</p>
            </>
          )}
          <div className="text-sm text-gray-600 mb-4 text-center">
            <p>
              {t.alreadyHaveAccount}{" "}
              <Link
                href="/auth/login"
                className="text-blue-600 hover:underline"
              >
                {t.login}
              </Link>
            </p>
          </div>
        </div>

        <button
          type="submit"
          className="bg-blue-600 text-white rounded py-2 font-semibold cursor-pointer"
        >
          {loading ? "Loading..." : <>{t.signUp}</>}
        </button>
        <div className="text-sm text-gray-600 text-center">
          {error && <p className="text-red-500 mb-2">{error}</p>}
        </div>
      </form>
    </div>
  );
}
