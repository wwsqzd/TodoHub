"use client";
import Link from "next/link";
import { useAuth } from "@/context/AuthContext";
import { RefObject, useRef } from "react";
import gsap from "gsap";

export default function Navbar() {
  const { accessToken, isAdmin } = useAuth();
  const logo = useRef<HTMLDivElement>(null);
  const nav1 = useRef<HTMLDivElement>(null);
  const nav2 = useRef<HTMLDivElement>(null);
  const nav3 = useRef<HTMLDivElement>(null);
  const nav4 = useRef<HTMLDivElement>(null);
  const nav5 = useRef<HTMLDivElement>(null);

  const handleEnter = (el: RefObject<HTMLDivElement | null>) => {
    gsap.to(el.current, {
      duration: 0.5,
      rotate: gsap.utils.random([-4, 4], true),
    });
  };

  const handleLeave = (el: RefObject<HTMLDivElement | null>) => {
    gsap.to(el.current, {
      duration: 0.5,
      rotate: 0,
    });
  };

  return (
    <nav className="w-full h-[110px] justify-between flex bg-white text-white border-b border-gray-200 border-solid">
      <div
        ref={logo}
        onMouseEnter={() => handleEnter(logo)}
        onMouseLeave={() => handleLeave(logo)}
        className="flex items-center cursor-pointer"
      >
        <h1 className="text-4xl font-bold text-black m-6">TodoHub</h1>
      </div>
      <div className="flex gap-10 items-center m-6 pr-10">
        {!accessToken ? (
          <>
            <Link href="/" className="text-black font-bold text-lg">
              <div
                ref={nav1}
                onMouseEnter={() => handleEnter(nav1)}
                onMouseLeave={() => handleLeave(nav1)}
              >
                Home
              </div>
            </Link>
            <Link href="/auth/login" className="text-black font-bold text-lg">
              <div
                ref={nav2}
                onMouseEnter={() => handleEnter(nav2)}
                onMouseLeave={() => handleLeave(nav2)}
              >
                Login
              </div>
            </Link>
          </>
        ) : (
          <>
            <Link href="/dashboard" className="text-black font-bold text-lg">
              <div
                ref={nav3}
                onMouseEnter={() => handleEnter(nav3)}
                onMouseLeave={() => handleLeave(nav3)}
              >
                Dashboard
              </div>
            </Link>
            {isAdmin && (
              <Link
                href="/admin/users"
                className="text-black font-bold text-lg"
              >
                <div
                  ref={nav4}
                  onMouseEnter={() => handleEnter(nav4)}
                  onMouseLeave={() => handleLeave(nav4)}
                >
                  Admin
                </div>
              </Link>
            )}
            <Link href="/profile" className="text-black font-bold text-lg">
              <div
                ref={nav5}
                onMouseEnter={() => handleEnter(nav5)}
                onMouseLeave={() => handleLeave(nav5)}
              >
                Profile
              </div>
            </Link>
          </>
        )}
      </div>
    </nav>
  );
}
