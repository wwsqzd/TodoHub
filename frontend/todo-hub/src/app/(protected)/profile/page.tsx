"use client";

import { useEffect, useState } from "react";
import { getMe, logOut } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";
import axios from "axios";
import { CiUser } from "react-icons/ci";
import LoadingUI from "@/components/ui/LoadingUI";
import { Profile } from "@/types";
import Image from "next/image";

export default function ProfilePage() {
  const [profile, setProfile] = useState<Profile>();
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const { accessToken, setAccessToken } = useAuth();

  useEffect(() => {
    const fetchData = async () => {
      if (!accessToken) {
        return;
      }
      try {
        const res = await getMe();
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
      window.location.href = "/auth/login";
    } catch (err) {
      console.error("Logout failed:", err);
    }
  };

  return (
    <div className="min-h-screen flex flex-col items-center">
      {error && <p className="text-red-500">{error}</p>}
      <div className="bg-white min-h-44 rounded shadow p-6 w-full h-full max-w-md flex flex-col items-center justify-center m-10 relative">
        {loading ? (
          <LoadingUI />
        ) : profile ? (
          <>
            <h1 className="mb-6 text-2xl font-bold">Account Profile</h1>
            <div className="w-[70px] h-[70px]">
              {profile.pictureUrl ? (
                <Image
                  src={profile.pictureUrl}
                  alt="profile img"
                  width={70}
                  height={70}
                />
              ) : (
                <CiUser className="w-full h-full text-gray-400" />
              )}
            </div>
            <div className="w-full flex flex-col gap-2 mt-4">
              <p className="text-left text-sm">Name: {profile.name}</p>
              <p className="text-left text-sm">Email: {profile.email}</p>
            </div>
            <button
              className="mt-4 bg-red-800 text-white rounded py-2 px-4 font-semibold cursor-pointer"
              onClick={handleLogout}
            >
              Log out
            </button>
          </>
        ) : (
          <></>
        )}
      </div>
    </div>
  );
}
