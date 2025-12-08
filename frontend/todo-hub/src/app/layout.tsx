import type { Metadata } from "next";
import "./globals.css";
import Navbar from "@/components/layout/Navbar";
import Footer from "@/components/layout/Footer";
import { AuthProvider } from "@/context/AuthContext";
import { Delius } from "next/font/google";
import { LanguageProvider } from "@/context/LanguageContext";

export const metadata: Metadata = {
  title: "TodoHub",
  description: "A simple todo app built with Next.js and Tailwind CSS",
};

const font = Delius({
  subsets: ["latin"],
  weight: ["400"],
});

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className={font.className}>
        <LanguageProvider>
          <AuthProvider>
            <Navbar />
            {children}
          </AuthProvider>
        </LanguageProvider>
        <Footer />
      </body>
    </html>
  );
}
