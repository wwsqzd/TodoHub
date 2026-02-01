
import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

export function middleware(request: NextRequest) {
  
  const accessToken = request.cookies.get("accessToken")?.value;
  const refreshToken = request.cookies.get("refreshToken")?.value;

  const isAuthPage = request.nextUrl.pathname.startsWith("/auth/login") 
                  || request.nextUrl.pathname.startsWith("/auth/register");

  const isProtectedPage = request.nextUrl.pathname.startsWith("/dashboard")
                        || request.nextUrl.pathname.startsWith("/profile");

  if ((accessToken || refreshToken) && isAuthPage) {
    return NextResponse.redirect(new URL("/dashboard", request.url));
  }

  if (isProtectedPage) {
    if (accessToken) return NextResponse.next();
    if (refreshToken) return NextResponse.next();
    return NextResponse.redirect(new URL("/auth/login", request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/dashboard/:path*", "/admin/:path*", "/profile", "/auth/login", "/auth/register"],
};