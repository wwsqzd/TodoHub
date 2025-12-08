"use client";

import React, { useState } from "react";
import { Profile } from "@/types";
import Image from "next/image";
import { CiUser } from "react-icons/ci";
import { useLanguage } from "@/context/LanguageContext";
import { translations } from "@/lib/dictionary";

interface Props {
  profile?: Profile;
}

export default function UserDetailsPart({ profile }: Props) {
  const [active, setActive] = useState<"info" | "settings">("info");

  const { language } = useLanguage();
  const t = translations[language];

  return (
    <div className="w-full">
      <div className="flex gap-6">
        {/* Left column: buttons */}
        <div className="w-[110px] flex flex-col gap-2">
          <button
            onClick={() => setActive("info")}
            className={`text-left cursor-pointer py-2 px-3 rounded transition-colors ${
              active === "info"
                ? "bg-blue-600 text-white"
                : "bg-transparent text-gray-700 hover:bg-gray-100"
            }`}
          >
            Info
          </button>
          <button
            onClick={() => setActive("settings")}
            className={`text-left cursor-pointer py-2 px-3 rounded transition-colors ${
              active === "settings"
                ? "bg-blue-600 text-white"
                : "bg-transparent text-gray-700 hover:bg-gray-100"
            }`}
          >
            {t.settings}
          </button>
        </div>

        {/* Right column: content */}
        <div className="flex-1 w-full">
          {active === "info" ? (
            <div>
              <div className="flex  items-center gap-4">
                <div className="w-[70px] h-[70px] overflow-hidden bg-white flex items-center justify-center">
                  {profile?.pictureUrl ? (
                    <Image
                      src={profile.pictureUrl}
                      alt="profile image"
                      width={70}
                      height={70}
                    />
                  ) : (
                    <CiUser className="w-full h-full text-gray-400" />
                  )}
                </div>

                <div className="flex-1 m-3">
                  <p className="text-sm">
                    <span>Hello {profile?.name ?? "—"}!</span>
                  </p>
                </div>
              </div>
            </div>
          ) : (
            <div>
              <div className="grid grid-cols-1 gap-3 m-5">
                <div>
                  <label className="text-sm block mb-1 text-gray-600">
                    Display Name
                  </label>
                  <p>{profile?.name}</p>
                </div>

                <div>
                  <label className="text-sm block mb-1 text-gray-600">
                    Email
                  </label>
                  <p>{profile?.email}</p>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
