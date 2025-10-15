"use client";
import CreateTodoModalWindow from "@/components/features/CreateTodoModalWindow";
import TodosList from "@/components/features/TodosList";
import WelcomePart from "@/components/features/WelcomePart";
import LoadingUI from "@/components/ui/LoadingUI";
import { getUserTodos } from "@/lib/api";
import { useState, useEffect } from "react";
import { Todo } from "@/types";
import ModifyTodoModalWindow from "@/components/features/ModifyTodoModalWindow";

export default function Dashboard() {
  const [todos, setTodos] = useState<Todo[]>([]);
  const [selectedTodo, setSelectedTodo] = useState<Todo | null>(null);
  const [showModalCreate, setShowModalCreate] = useState(false);
  const [showModalModify, setShowModalModify] = useState(false);
  const [loadingTodos, setLoadingTodos] = useState(true);

  useEffect(() => {
    const getTodos = async () => {
      const res = await getUserTodos();
      console.log(res.value);
      setTodos(res.value);
      setLoadingTodos(false);
    };
    getTodos();
  }, []);

  const handleCreate = (newTodo: Todo) => {
    setTodos((prev) => [newTodo, ...prev]);
  };

  const handleButton = () => {
    setShowModalCreate(true);
  };

  const handleDeleteTodo = (id: string) => {
    setTodos((prev) => prev.filter((t) => t.id !== id));
  };

  const handleModifyTodo = (updatedTodo: Todo) => {
    setTodos((prevTodos) =>
      prevTodos.map((todo) => (todo.id === updatedTodo.id ? updatedTodo : todo))
    );
  };

  const handleEditTodo = (todo: Todo) => {
    setSelectedTodo(todo);
    setShowModalModify(true);
  };

  const handleClose = () => {
    setSelectedTodo(null);
    setShowModalModify(false);
  };

  return (
    <>
      {showModalCreate && (
        <CreateTodoModalWindow
          isOpen={showModalCreate}
          onClose={() => setShowModalCreate(false)}
          onCreate={handleCreate}
        />
      )}
      {showModalModify && selectedTodo && (
        <ModifyTodoModalWindow
          todo={selectedTodo}
          isOpen={showModalModify}
          onClose={handleClose}
          onModify={handleModifyTodo}
        />
      )}

      {loadingTodos ? (
        <LoadingUI />
      ) : todos && todos.length > 0 ? (
        <TodosList
          todos={todos}
          handleButton={handleButton}
          onDelete={handleDeleteTodo}
          onModify={handleModifyTodo}
          onEdit={handleEditTodo}
        />
      ) : (
        <WelcomePart handleButton={handleButton} />
      )}
    </>
  );
}
