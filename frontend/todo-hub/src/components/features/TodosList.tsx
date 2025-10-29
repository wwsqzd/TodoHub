import React, { forwardRef } from "react";
import TodoItem from "./TodoItem";
import ButtonUI from "../ui/ButtonUI";
import { Todo } from "@/types";

type Props = {
  todos: Array<Todo>;
  handleButton: () => void;
  onDelete?: (id: string) => void;
  onModify?: (todo: Todo) => void;
  onEdit: (todo: Todo) => void;
};

const TodosList = forwardRef<HTMLDivElement, Props>(function TodosList(
  { todos, handleButton, onDelete, onModify, onEdit },
  ref
) {
  return (
    <div className="flex flex-col items-center pt-5 pb-20">
      <ButtonUI
        color="blue"
        text="Create Todo"
        w="w-44"
        h="h-11"
        action={handleButton}
      />
      <div
        ref={ref}
        className="min-h-screen columns-1 sm:columns-2 lg:columns-3 gap-4 p-4"
      >
        {todos.map((todo) => (
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
