import React, { forwardRef, useState, useMemo, useRef } from "react";
import TodoItem from "./TodoItem";
import ButtonUI from "../ui/ButtonUI";
import LoadingUI from "../ui/LoadingUI";
import { Todo } from "@/types";
import { useLanguage } from "@/context/LanguageContext";
import { translations } from "@/lib/dictionary";
import { FaSortAmountDown, FaSortAmountDownAlt } from "react-icons/fa";
import { IoIosSearch } from "react-icons/io";
import { searchTodos } from "@/lib/api";

type Props = {
  todos: Array<Todo>;
  handleButton: () => void;
  onDelete?: (id: string) => void;
  onModify?: (todo: Todo) => void;
  onEdit: (todo: Todo) => void;
};

type SortOrder = "newest" | "oldest";

const TodosList = forwardRef<HTMLDivElement, Props>(function TodosList(
  { todos, handleButton, onDelete, onModify, onEdit },
  ref
) {
  const { language } = useLanguage();
  const t = translations[language];

  const [sortOrder, setSortOrder] = useState<SortOrder>("newest");

  const sortedTodos = useMemo(() => {
    const sorted = [...todos].sort((a, b) => {
      const dateA = new Date(a.createdDate).getTime();
      const dateB = new Date(b.createdDate).getTime();
      return sortOrder === "oldest" ? dateB - dateA : dateA - dateB;
    });
    return sorted;
  }, [todos, sortOrder]);

  const [searchQuery, setSearchQuery] = useState("");
  const [filteredTodos, setFilteredTodos] = useState<Todo[]>([]);
  const [searching, setSearching] = useState(false);
  const searchTimeoutRef = useRef<number | null>(null);

  const SeearchTodo = async (query: string) => {
    setSearchQuery(query);

    if (searchTimeoutRef.current) {
      window.clearTimeout(searchTimeoutRef.current);
    }

    // debounce the API call
    searchTimeoutRef.current = window.setTimeout(async () => {
      const q = query.trim();
      if (!q) {
        setFilteredTodos([]);
        setSearching(false);
        return;
      }

      setSearching(true);
      try {
        const res = await searchTodos(q);
        const results: Todo[] = res?.value ?? res ?? [];
        setFilteredTodos(results);
      } catch (err) {
        console.error(err);
        setFilteredTodos([]);
      } finally {
        setSearching(false);
      }
    }, 300);
  };

  const displayedTodos = useMemo(() => {
    if (searchQuery.trim()) {
      const sorted = [...filteredTodos].sort((a, b) => {
        const dateA = new Date(a.createdDate).getTime();
        const dateB = new Date(b.createdDate).getTime();
        return sortOrder === "oldest" ? dateB - dateA : dateA - dateB;
      });
      return sorted;
    }
    return sortedTodos;
  }, [searchQuery, filteredTodos, sortedTodos, sortOrder]);

  return (
    <div className="flex flex-col items-center mt-3 min-h-[calc(100vh-110px)] w-full">
      <div className="flex items-center">
        <input
          type="text"
          value={searchQuery}
          onChange={(e) => SeearchTodo(e.target.value)}
          placeholder={"Search todos "}
          className="w-96 max-w-md px-4 py-2 border border-gray-300 rounded-lg mb-4 "
        />
      </div>
      <div className="flex gap-4 mb-4">
        <ButtonUI
          color="blue"
          text={t.createTodo}
          w="w-44"
          h="h-11"
          action={handleButton}
        />
        <button
          onClick={() =>
            setSortOrder(sortOrder === "newest" ? "oldest" : "newest")
          }
          className="px-4 py-2  text-gray-800 rounded cursor-pointer transition-colors font-medium text-sm flex items-center gap-2"
          title={sortOrder === "newest" ? t.sortNewest : t.sortOldest}
        >
          {sortOrder === "newest" ? (
            <FaSortAmountDown size={18} />
          ) : (
            <FaSortAmountDownAlt size={18} />
          )}
        </button>
      </div>
      <div ref={ref} className="flex items-center  flex-col gap-4 p-4 w-full">
        {displayedTodos.map((todo) => (
          // <div key={todo.id} data-id={todo.id} className="min-w-[150px] relative">
          <TodoItem
            key={todo.id}
            todo={todo}
            onDelete={onDelete}
            onModify={onModify}
            onEdit={onEdit}
          />
          // </div>
        ))}
      </div>
    </div>
  );
});

export default TodosList;
