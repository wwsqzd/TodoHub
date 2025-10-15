"use client";
import { useAuth } from "@/context/AuthContext";
import { GetUsers } from "@/lib/api";
import { useEffect, useState } from "react";
import { Users } from "@/types";

export default function AdminPage() {
  const { isAdmin } = useAuth();
  const [usersData, SetUsersData] = useState([]);

  useEffect(() => {
    const getUsers = async () => {
      const res = await GetUsers();
      console.log(res);
      SetUsersData(res);
    };
    getUsers();
  }, []);

  return (
    <>
      {isAdmin &&
        (usersData.length > 0 ? (
          <div className="min-h-screen flex flex-col items-center bg-gray-50 py-10">
            <h2 className="text-3xl font-bold mb-8 text-blue-500">
              Users List
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8 w-full max-w-5xl">
              {usersData.map((user: Users) => (
                <div
                  key={user.id}
                  className="bg-white rounded-lg shadow-md p-6 flex flex-col items-start border border-gray-200 hover:shadow-lg transition"
                >
                  <div className="font-bold text-xl mb-2 text-gray-900">
                    {user.name}
                  </div>
                  <div className="text-gray-500 mb-1">{user.email}</div>
                  <div
                    className={`text-sm font-semibold mb-2 ${
                      user.isAdmin ? "text-blue-600" : "text-green-600"
                    }`}
                  >
                    {user.isAdmin ? "Admin" : "User"}
                  </div>
                  <div className="text-xs text-gray-400">
                    Joined: {new Date(user.createdAt).toLocaleDateString()}
                  </div>
                </div>
              ))}
            </div>
          </div>
        ) : (
          <div className="min-h-screen flex items-center justify-center bg-gray-50">
            <p className="text-lg text-gray-500">No users found.</p>
          </div>
        ))}
    </>
  );
}
