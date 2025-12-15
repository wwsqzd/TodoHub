"use client";
import Link from "next/link";
import { useAuth } from "@/context/AuthContext";
import { useLanguage } from "@/context/LanguageContext";
import { RefObject, useRef, useState, useEffect } from "react";
import { HiMenu } from "react-icons/hi";
import { HiX } from "react-icons/hi";
import gsap from "gsap";
import { translations } from "@/lib/dictionary";

export default function Navbar() {
  const { accessToken, isAdmin, loading } = useAuth();
  const { language } = useLanguage();
  const t = translations[language];
  const logo = useRef<HTMLDivElement>(null);
  const nav1 = useRef<HTMLDivElement>(null);
  const nav2 = useRef<HTMLDivElement>(null);
  const nav3 = useRef<HTMLDivElement>(null);
  const nav4 = useRef<HTMLDivElement>(null);
  const nav5 = useRef<HTMLDivElement>(null);

  // Mobile menu refs & state
  const [menuOpen, setMenuOpen] = useState(false);
  const menuPanel = useRef<HTMLDivElement | null>(null);
  const menuBtn = useRef<HTMLButtonElement | null>(null);
  const tl = useRef<GSAPTimeline | null>(null);

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

  useEffect(() => {
    if (!menuPanel.current) return;

    // create timeline once
    if (!tl.current) {
      const panel = menuPanel.current;
      const items = panel.querySelectorAll(".mobile-nav-item");

      tl.current = gsap.timeline({ paused: true });
      tl.current.to(panel, {
        duration: 0.3,
        height: "auto",
        opacity: 1,
        ease: "power2.out",
        overwrite: true,
      });
      tl.current.from(
        items,
        {
          y: -8,
          opacity: 0,
          duration: 0.25,
          stagger: 0.05,
          ease: "power2.out",
        },
        "-=0.15"
      );

      // set initial closed state (height 0, hidden)
      gsap.set(menuPanel.current, {
        height: 0,
        opacity: 0,
        overflow: "hidden",
      });
    }

    // open/close based on menuOpen
    if (menuOpen) {
      tl.current!.play();
    } else {
      tl.current!.reverse();
    }
  }, [menuOpen]);

  // click outside to close
  useEffect(() => {
    function onDocClick(e: MouseEvent) {
      const target = e.target as Node;
      if (!menuOpen) return;
      if (
        menuPanel.current &&
        !menuPanel.current.contains(target) &&
        menuBtn.current &&
        !menuBtn.current.contains(target)
      ) {
        setMenuOpen(false);
      }
    }
    document.addEventListener("mousedown", onDocClick);
    return () => document.removeEventListener("mousedown", onDocClick);
  }, [menuOpen]);

  return (
    <nav className="w-full h-[110px] flex items-center bg-white border-b border-gray-200 border-solid relative z-50 justify-between">
      {/* Logo - Left */}
      <div
        ref={logo}
        onMouseEnter={() => handleEnter(logo)}
        onMouseLeave={() => handleLeave(logo)}
        className="flex items-center cursor-pointer flex-shrink-0"
      >
        <h1 className="text-4xl font-bold text-black m-6">TodoHub</h1>
      </div>

      {/* Desktop Nav - Center */}
      <div className="hidden sm:flex gap-10 items-center flex-1 justify-center max-w-lg">
        {!accessToken ? (
          <>
            <Link href="/" className="text-black font-bold text-lg">
              <div
                ref={nav1}
                onMouseEnter={() => handleEnter(nav1)}
                onMouseLeave={() => handleLeave(nav1)}
              >
                {t.home}
              </div>
            </Link>
            <Link href="/auth/login" className="text-black font-bold text-lg">
              <div
                ref={nav2}
                onMouseEnter={() => handleEnter(nav2)}
                onMouseLeave={() => handleLeave(nav2)}
              >
                {t.login}
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
                {t.dashboard}
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
                  {t.admin}
                </div>
              </Link>
            )}
            <Link href="/profile" className="text-black font-bold text-lg">
              <div
                ref={nav5}
                onMouseEnter={() => handleEnter(nav5)}
                onMouseLeave={() => handleLeave(nav5)}
              >
                {t.profile}
              </div>
            </Link>
          </>
        )}
      </div>

      {/* Mobile Controls - Right */}
      <div className="flex items-center gap-3 ml-auto sm:hidden flex-shrink-0">
        {/* Mobile Menu Button */}
        <div className="w-[36px] h-[36px] m-2">
          <button
            ref={menuBtn}
            aria-expanded={menuOpen}
            onClick={() => setMenuOpen((s) => !s)}
            className="text-black"
          >
            {menuOpen ? (
              <HiX size={36} className="cursor-pointer" />
            ) : (
              <HiMenu size={36} className="cursor-pointer" />
            )}
          </button>
        </div>
      </div>

      {/* Mobile Menu Panel */}
      <div
        ref={menuPanel}
        className="sm:hidden absolute left-0 right-0 top-[110px] bg-white shadow-md overflow-hidden"
      >
        <div className="flex flex-col py-4">
          {!accessToken ? (
            <>
              <Link href="/">
                <div
                  className="mobile-nav-item px-6 py-3 text-black font-bold"
                  onClick={() => setMenuOpen(false)}
                >
                  {t.home}
                </div>
              </Link>
              <Link href="/auth/login">
                <div
                  className="mobile-nav-item px-6 py-3 text-black font-bold"
                  onClick={() => setMenuOpen(false)}
                >
                  {t.login}
                </div>
              </Link>
            </>
          ) : (
            <>
              <Link href="/dashboard">
                <div
                  className="mobile-nav-item px-6 py-3 text-black font-bold"
                  onClick={() => setMenuOpen(false)}
                >
                  {t.dashboard}
                </div>
              </Link>
              {isAdmin && (
                <Link href="/admin/users">
                  <div
                    className="mobile-nav-item px-6 py-3 text-black font-bold"
                    onClick={() => setMenuOpen(false)}
                  >
                    {t.admin}
                  </div>
                </Link>
              )}
              <Link href="/profile">
                <div
                  className="mobile-nav-item px-6 py-3 text-black font-bold"
                  onClick={() => setMenuOpen(false)}
                >
                  {t.profile}
                </div>
              </Link>
            </>
          )}
        </div>
      </div>
    </nav>
  );
}
