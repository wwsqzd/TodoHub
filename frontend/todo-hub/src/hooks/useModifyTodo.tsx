import { modifyTodo } from "@/lib/api";
import { useState } from "react";

export function useModifyTodo() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const modify = async (
    id: string,
    data: { title: string; description: string; isCompleted: boolean }
  ) => {
    setError(null);
    setLoading(true);

    if (data.title.length < 5 || data.title.length > 40) {
      setLoading(false);
      setError(
        "The length of the title must be at least 5 and no more than 40 characters."
      );
      return;
    }

    try {
      const res = await modifyTodo(id, data);
      if (res) {
        return res.value;
      }
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message);
      }
    }
  };

  return { modify, error, loading };
}
