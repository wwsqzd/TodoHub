"use client";
import CreateTodoModalWindow from "@/components/features/CreateTodoModalWindow";
import TodosList from "@/components/features/TodosList";
import WelcomePart from "@/components/features/WelcomePart";
import LoadingUI from "@/components/ui/LoadingUI";
import { getUserTodos } from "@/lib/api";
import { useState, useEffect, useRef, useLayoutEffect } from "react";
import { Todo } from "@/types";
import ModifyTodoModalWindow from "@/components/features/ModifyTodoModalWindow";
import { useInView } from "react-intersection-observer";
import gsap from "gsap";
import { Flip } from "gsap/Flip";

gsap.registerPlugin(Flip);

export default function Dashboard() {
  const [todos, setTodos] = useState<Todo[]>([]);
  const [selectedTodo, setSelectedTodo] = useState<Todo | null>(null);
  const [showModalCreate, setShowModalCreate] = useState(false);
  const [showModalModify, setShowModalModify] = useState(false);
  const [loadingTodos, setLoadingTodos] = useState(true);
  const [isFetchingMore, setIsFetchingMore] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [lastCreated, setLastCreated] = useState<Date | null>(null);
  const [lastId, setLastId] = useState<number | null>(null);
  const { ref: sentinelRef, inView } = useInView({
    threshold: 0,
    rootMargin: "100px",
    delay: 500,
  });
  const ConRef = useRef<HTMLDivElement>(null);
  const flipState = useRef<Flip.FlipState | null>(null);
  const [isInitialLoad, setIsInitialLoad] = useState(true);

  // get todos
  const getTodos = async () => {
    try {
      const params = new URLSearchParams();
      if (lastCreated && lastId) {
        params.append("lastCreated", lastCreated.toISOString());
        params.append("lastId", lastId.toString());
      }
      const res = await getUserTodos(params);
      if (res.value.length === 0) {
        setHasMore(false);
        return;
      }

      setTodos((prev) => {
        const combined = [...prev, ...res.value];
        const unique = Array.from(
          new Map(combined.map((t) => [t.id, t])).values()
        );
        return unique;
      });

      const last = res.value[res.value.length - 1];
      setLastCreated(new Date(last.createdDate));
      setLastId(last.id);

      if (res.value.length < 10) {
        setHasMore(false);
      }
    } finally {
      setLoadingTodos(false);
      setIsFetchingMore(false);
      setIsInitialLoad(false);
    }
  };

  useEffect(() => {
    if (isInitialLoad) {
      setLoadingTodos(true);
      getTodos();
    }
  }, [isInitialLoad]);

  useEffect(() => {
    if (!isInitialLoad && inView && hasMore && !isFetchingMore) {
      setIsFetchingMore(true);
      if (ConRef.current) {
        const todosElements = ConRef.current.querySelectorAll(
          "div.break-inside-avoid"
        );
        flipState.current = Flip.getState(todosElements);
      }
      getTodos();
    }
  }, [todos, inView, hasMore, isFetchingMore, isInitialLoad]);

  // handle create
  const handleCreate = (newTodo: Todo) => {
    setTodos((prev) => [newTodo, ...prev]);
  };

  // handle create modal
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
    const animateLoad = async () => {
      if (flipState.current) {
        await Flip.from(flipState.current, {
          duration: 0.8,
          ease: "power1.inOut",
          absolute: true,
          stagger: 0.05,
        });
        flipState.current = null;
      }
    };
    animateLoad();
    setIsFetchingMore(false);
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
        <div className="w-[100vw - 30px] h-screen p-6 flex justify-center">
          <div className="h-72 w-[550px]">
            <LoadingUI />
          </div>
        </div>
      ) : todos && todos.length > 0 ? (
        <div className="flex flex-col justify-between min-h-[calc(100vh-110px)]">
          <TodosList
            ref={ConRef}
            todos={todos}
            handleButton={handleButton}
            onDelete={handleDeleteTodo}
            onModify={handleModifyTodo}
            onEdit={handleEditTodo}
          />
          {hasMore && (
            <div ref={sentinelRef} className="w-full py-4 flex justify-center">
              {isFetchingMore && <LoadingUI />}
            </div>
          )}
        </div>
      ) : (
        <WelcomePart handleButton={handleButton} />
      )}
    </>
  );
}
