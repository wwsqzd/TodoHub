"use client";

import { useEffect, useState } from "react";
import { getMe, logOut } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";
import axios from "axios";

import LoadingUI from "@/components/ui/LoadingUI";
import { Profile } from "@/types";

import { useLanguage } from "@/context/LanguageContext";
import { translations } from "@/lib/dictionary";
import UserDetailsPart from "@/components/features/UserDetailsPart";

export default function ProfilePage() {
  const [profile, setProfile] = useState<Profile>();
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const { accessToken, setAccessToken } = useAuth();

  const { language, setLanguage } = useLanguage();
  const t = translations[language];

  useEffect(() => {
    const fetchData = async () => {
      if (!accessToken) {
        return;
      }
      try {
        const res = await getMe();
        console.log(res);
        const lang = res.value?.interface_Language;
        console.log(lang);
        if (lang === "en" || lang === "de") {
          setLanguage(lang);
        }
        setProfile(res.value);
        setLoading(false);
      } catch (err: unknown) {
        if (axios.isAxiosError(err)) {
          if (err.response?.status === 409) {
            return;
          }
        }
        if (err instanceof Error) {
          setError(err.message || "An unexpected error occurred");
        } else {
          setError("Unknown error");
        }
      }
    };

    fetchData();
  }, [accessToken, setAccessToken]);

  const handleLogout = async () => {
    try {
      await logOut();
      setAccessToken(null);
      document.cookie =
        "accessToken=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT";
      setProfile(undefined);
      localStorage.clear();
      window.location.href = "/auth/login";
    } catch (err) {
      console.error("Logout failed:", err);
    }
  };

  return (
    <div className="min-h-screen flex flex-col items-center">
      {error && <p className="text-red-500">{error}</p>}
      <div className="bg-white min-h-44 rounded shadow p-3 sm:w-[600px] w-[80vw] h-full  flex flex-col items-center justify-center m-10 relative">
        {loading ? (
          <LoadingUI />
        ) : profile ? (
          <>
            <h1 className="mb-6 text-2xl font-bold">{t.accountProfile}</h1>
            <UserDetailsPart profile={profile} />
            <button
              className="mt-4 bg-red-800 text-white rounded py-2 px-4 font-semibold cursor-pointer"
              onClick={handleLogout}
            >
              {t.logout}
            </button>
          </>
        ) : (
          <></>
        )}
      </div>
    </div>
  );
}
