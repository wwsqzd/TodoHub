import TodoItem from "./TodoItem";
import ButtonUI from "../ui/ButtonUI";
import { Todo } from "@/types";
import { RefObject } from "react";

type Props = {
  todos: Array<Todo>;
  ref: RefObject<HTMLDivElement | null>;
  handleButton: () => void;
  onDelete?: (id: string) => void;
  onModify?: (todo: Todo) => void;
  onEdit: (todo: Todo) => void;
};

export default function TodosList({
  todos,
  ref,
  handleButton,
  onDelete,
  onModify,
  onEdit,
}: Props) {
  // const ConRef = useRef<HTMLDivElement>(null);
  // const state = Flip.getState(ConRef.current);
  // const onDeleteAnim = async (id) => {
  //   await Flip.from(state, { duration: 2, ease: "power1.inOut" });
  //   onDelete?.(id);
  // };

  return (
    <div className="flex flex-col items-center pt-5">
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
}
