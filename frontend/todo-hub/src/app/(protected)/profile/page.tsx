"use client";

import { useEffect, useState } from "react";
import { getMe, logOut } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";

type Profile = {
    name: string;
    email: string;
    IsAdmin: boolean;
};

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
        console.log("Profile data:", res);
        setProfile(res.value);
        setLoading(false);

    } catch (err: unknown) {    
      if (err instanceof Error) {
        setError(err.message || "An unexpected error occurred");
      } else {
        setError("Unknown error");
      }
    }
  };

  fetchData();
}, [accessToken, setAccessToken]);

    const handleLogout = async () => 
    {
        try {
            await logOut();
            setAccessToken(null);
            setProfile(undefined);
            document.cookie = "accessToken=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT"; // Clear cookie
            window.location.href = "/auth/login";
        } catch (err) 
        {
            console.error("Logout failed:", err);
        }
    }

  return (
        <div className="flex flex-col items-center">
      {error && <p className="text-red-500">{error}</p>}
      {loading ? (
        <p>Loading profile...</p>
      ) : profile ? (
        <div className="bg-white rounded shadow p-6 w-full max-w-md flex flex-col items-center justify-center">
            <h1 className="mb-6 text-2xl font-bold">Profile</h1>
            <div className="w-full">
                <p className="text-left text-sm">Name: {profile.name}</p>
                <p className="text-left text-sm">Email: {profile.email}</p>
                <p className="text-left text-sm">Admin: {profile.IsAdmin ? "No" : "Yes"}</p>
            </div>
          <button className="mt-4 bg-blue-600 text-white rounded py-2 px-4 font-semibold cursor-pointer" onClick={handleLogout}>
            Log out 
            </button>
        </div>
      ) : (
        <p>No profile data found.</p>
      )}
    </div>
  );
}