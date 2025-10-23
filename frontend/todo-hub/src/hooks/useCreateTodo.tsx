import { useState } from "react";
import { createTodo } from "@/lib/api";

export function useCreateTodo() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const create = async (data: { title: string; description: string }) => {
    setError(null);
    setLoading(true);

    if (data.title.length < 5 || data.title.length > 40) {
      setLoading(false);
      setError(
        "The length of the title must be at least 5 and no more than 40 characters."
      );
      return;
    }
    if (data.description.length > 300) {
      setLoading(false);
      setError(
        "The length of thedescription must be no more than 300 characters."
      );
      return;
    }
    try {
      const newTodo = await createTodo(data);
      return newTodo.value;
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message);
      }
    } finally {
      setLoading(false);
    }
  };

  return { create, loading, error };
}
