import React, { forwardRef, useState, useMemo } from "react";
import TodoItem from "./TodoItem";
import ButtonUI from "../ui/ButtonUI";
import { Todo } from "@/types";
import { useLanguage } from "@/context/LanguageContext";
import { translations } from "@/lib/dictionary";
import { FaSortAmountDown, FaSortAmountDownAlt } from "react-icons/fa";

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

  return (
    <div className="flex flex-col items-center pt-5 pb-20">
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
      <div
        ref={ref}
        className="min-h-1/6 columns-1 sm:columns-2 lg:columns-3 gap-4 p-4"
      >
        {sortedTodos.map((todo) => (
          <div
            key={todo.id}
            data-id={todo.id}
            className="break-inside-avoid mb-4 w-[250px]"
          >
            <TodoItem
              todo={todo}
              onDelete={onDelete}
              onModify={onModify}
              onEdit={onEdit}
            />
          </div>
        ))}
      </div>
    </div>
  );
});

export default TodosList;
