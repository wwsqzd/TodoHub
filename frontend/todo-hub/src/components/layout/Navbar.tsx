'use client';
import Link from "next/link";
import { useAuth } from "@/context/AuthContext";

export default function Navbar() {

   const { accessToken } = useAuth();

    return (
        <nav className="w-full h-[110px] justify-between flex bg-white text-white border-b border-gray-200 border-solid">
            <div className="flex items-center">
                <h1 className="text-4xl font-bold text-black m-6">TodoHub</h1>
            </div>
            <div className="flex gap-10 items-center m-6 pr-10">
                {!accessToken ? (
                    <>
                        <Link href="/" className="text-black font-bold text-lg">Home</Link>
                        <Link href="/auth/login" className="text-black font-bold text-lg">Login</Link>
                    </>
                ) : (
                    <>
                        <Link href="/dashboard" className="text-black font-bold text-lg">Dashboard</Link> 
                        <Link href="/profile" className="text-black font-bold text-lg">Profile</Link>
                    </>
                )}
            </div>
        </nav>
    )
}