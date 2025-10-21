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
      SetUsersData(res);
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
          <div className=" flex items-center justify-center bg-gray-50">
            <LoadingUI />
          </div>
        ))}
    </>
  );
}
