import { DeleteUser } from "@/lib/api";
import { useState } from "react";

export function useDeleteUser() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const deleteUser = async (id: string) => {
    setError(null);
    setLoading(true);
    try {
      const deletedUserId = await DeleteUser(id);
      return deletedUserId.value;
    } catch (err) {
      if (err instanceof Error) {
        console.log(err);
      }
    } finally {
      setLoading(false);
    }
  };

  return { deleteUser, loading, error };
}
