import { useState } from "react";
import { deleteTodo } from "@/lib/api";

export function useDeleteTodo() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const delTodo = async (id: string) => {
    setError(null);
    setLoading(true);

    try {
      const deletedTodoId = await deleteTodo(id);
      return deletedTodoId.value;
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message);
      }
    } finally {
      setLoading(false);
    }
  };

  return { delTodo, loading, error };
}
