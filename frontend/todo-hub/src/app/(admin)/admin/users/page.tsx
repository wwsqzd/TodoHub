"use client";
import { useAuth } from "@/context/AuthContext";
import { GetUsers } from "@/lib/api";
import { useEffect, useState } from "react";
import UserList from "@/components/features/UserList";
import LoadingUI from "@/components/ui/LoadingUI";
import { Users } from "@/types";

export default function AdminPage() {
  const { isAdmin } = useAuth();
  const [usersData, SetUsersData] = useState<Users[]>([]);

  useEffect(() => {
    const getUsers = async () => {
      const res = await GetUsers();
      SetUsersData(res.value);
    };
    getUsers();
  }, []);

  const handleDeleteUser = (id: string) => {
    SetUsersData((prev) => prev.filter((u) => u.id !== id));
  };

  return (
    <>
      {isAdmin &&
        (usersData.length > 0 ? (
          <UserList usersData={usersData} onDelete={handleDeleteUser} />
        ) : (
          <div className="w-[100vw - 30px] h-screen p-6 flex justify-center">
            <div className="h-72 w-[550px]">
              <LoadingUI />
            </div>
          </div>
        ))}
    </>
  );
}
