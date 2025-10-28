"use client";
import CreateTodoModalWindow from "@/components/features/CreateTodoModalWindow";
import TodosList from "@/components/features/TodosList";
import WelcomePart from "@/components/features/WelcomePart";
import LoadingUI from "@/components/ui/LoadingUI";
import { getUserTodos } from "@/lib/api";
import { useState, useEffect, useRef, useLayoutEffect } from "react";
import { Todo } from "@/types";
import ModifyTodoModalWindow from "@/components/features/ModifyTodoModalWindow";

import gsap from "gsap";
import { Flip } from "gsap/Flip";

gsap.registerPlugin(Flip);

export default function Dashboard() {
  const [todos, setTodos] = useState<Todo[]>([]);
  const [selectedTodo, setSelectedTodo] = useState<Todo | null>(null);
  const [showModalCreate, setShowModalCreate] = useState(false);
  const [showModalModify, setShowModalModify] = useState(false);
  const [loadingTodos, setLoadingTodos] = useState(true);

  useEffect(() => {
    const getTodos = async () => {
      const res = await getUserTodos();
      console.log(res);
      setTodos(res.value);
      setLoadingTodos(false);
    };
    getTodos();
  }, []);

  const ConRef = useRef<HTMLDivElement>(null);
  const flipState = useRef<Flip.FlipState | null>(null);

  const handleCreate = (newTodo: Todo) => {
    if (!ConRef.current) return;
    const todosElements = ConRef.current.querySelectorAll(
      "div.break-inside-avoid"
    );
    flipState.current = Flip.getState(todosElements);
    setTodos((prev) => [newTodo, ...prev]);
  };

  const handleButton = () => {
    setShowModalCreate(true);
  };

  // delete with animation
  const handleDeleteTodo = (id: string) => {
    if (!ConRef.current) return;
    const todosElements = ConRef.current.querySelectorAll(
      "div.break-inside-avoid"
    );
    flipState.current = Flip.getState(todosElements);
    setTodos((prev) => prev.filter((t) => t.id !== id));
  };

  // animation
  useLayoutEffect(() => {
    if (flipState.current) {
      Flip.from(flipState.current, {
        duration: 0.6,
        ease: "power1.inOut",
        absolute: true,
        stagger: 0.05,
      });
      flipState.current = null;
    }
  }, [todos]);

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
          ref={ConRef}
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
