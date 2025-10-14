import { useState } from "react";
import { createTodo } from "@/lib/api";


export function useCreateTodo() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const create = async (data: { title: string; description: string }) => {
      setError(null);
      setLoading(true);
    
      if (data.title.length < 5) {
        setLoading(false);  
        setError("The length of the title must be at least 5 characters.");
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

    return {create, loading, error}
}