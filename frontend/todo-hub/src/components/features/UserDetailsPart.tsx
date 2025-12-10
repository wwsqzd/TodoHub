"use client";

import React, { useState } from "react";
import { Profile } from "@/types";
import Image from "next/image";
import { CiUser } from "react-icons/ci";
import { useLanguage } from "@/context/LanguageContext";
import { translations } from "@/lib/dictionary";
import { ChangeUserlanguage } from "@/lib/api";

interface Props {
  profile?: Profile;
}

export default function UserDetailsPart({ profile }: Props) {
  const [active, setActive] = useState<"info" | "settings">("info");

  const { language, setLanguage } = useLanguage();
  const t = translations[language];

  const changelanguage = async (lang: "en" | "de") => {
    const res = await ChangeUserlanguage(lang);
    console.log(res);
    if (res.success) {
      setLanguage(lang);
    }
  };

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
                <div className="w-[70px] h-[70px] border-2 border-gray-200 overflow-hidden bg-white flex items-center justify-center">
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
                <div className="border-t pt-4">
                  <label className="text-sm block mb-3 text-gray-600">
                    Language
                  </label>
                  <div className="flex gap-2">
                    <button
                      onClick={() => changelanguage("en")}
                      className={`px-4 py-2 rounded cursor-pointer font-bold transition text-sm ${
                        language === "en"
                          ? "bg-black text-white"
                          : "bg-gray-100 text-black hover:bg-gray-200"
                      }`}
                    >
                      English
                    </button>
                    <button
                      onClick={() => changelanguage("de")}
                      className={`px-4 py-2 rounded cursor-pointer font-bold transition text-sm ${
                        language === "de"
                          ? "bg-black text-white"
                          : "bg-gray-100 text-black hover:bg-gray-200"
                      }`}
                    >
                      Deutsch
                    </button>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
