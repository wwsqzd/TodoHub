
import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

export function middleware(request: NextRequest) {
  
  const token = request.cookies.get("accessToken")?.value;

  const isAuthPage = request.nextUrl.pathname.startsWith("/auth/login") 
                  || request.nextUrl.pathname.startsWith("/auth/register");

  const isProtectedPage = request.nextUrl.pathname.startsWith("/dashboard")
                        || request.nextUrl.pathname.startsWith("/profile");

  // Если токен есть → не пускать на /login, /register
  if (token && isAuthPage) {
    console.log("Redirecting to /dashboard because user is authenticated");
    return NextResponse.redirect(new URL("/dashboard", request.url));
  }

  // Если токена нет → не пускать на приватные маршруты
  if (!token && isProtectedPage) {
    console.log("Redirecting to /auth/login because user is not authenticated");
    return NextResponse.redirect(new URL("/auth/login", request.url));
  }

  // Если нет токена, но лезет в админку
  if (!token && request.nextUrl.pathname.startsWith("/admin")) {
    return NextResponse.redirect(new URL("/auth/login", request.url));
  }

  // Если есть токен, но роль не admin
//   if (token && request.nextUrl.pathname.startsWith("/admin")) {
//     // тут можно раскодировать JWT и проверить роль
//     const role = "user"; // TODO: вытащить из токена
//     if (role !== "admin") {
//       return NextResponse.redirect(new URL("/dashboard", request.url));
//     }
//   }

  return NextResponse.next();
}

// Укажем, на какие роуты распространяется middleware
export const config = {
  matcher: ["/dashboard/:path*", "/admin/:path*", "/auth/login", "/auth/register", "/profile"],
};