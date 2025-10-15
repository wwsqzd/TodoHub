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

export default function TodosList({
  todos,
  handleButton,
  onDelete,
  onModify,
  onEdit,
}: Props) {
  function splitArrayIntoThree(arr: Array<Todo>) {
    const n = arr.length;
    const minSize = Math.floor(n / 3);
    let remainder = n % 3;

    const result = [];
    let start = 0;

    for (let i = 0; i < 3; i++) {
      let size = minSize;
      if (remainder > 0) {
        size += 1;
        remainder -= 1;
      }
      result.push(arr.slice(start, start + size));
      start += size;
    }

    return result;
  }
  const [part1, part2, part3] = splitArrayIntoThree(todos);

  return (
    <div className="flex flex-col items-center pt-5">
      <ButtonUI
        color="blue"
        text="Create Todo"
        w="w-44"
        h="h-11"
        action={handleButton}
      />
      <div className="min-h-screen flex justify-center gap-6 py-10">
        <div className="flex flex-col justify-start w-2xs gap-4">
          {part1.map((todo: Todo) => (
            <TodoItem
              key={todo.id}
              todo={todo}
              onDelete={onDelete}
              onModify={onModify}
              onEdit={onEdit}
            />
          ))}
        </div>
        <div className="flex flex-col justify-start w-2xs gap-4">
          {part2.map((todo: Todo) => (
            <TodoItem
              key={todo.id}
              todo={todo}
              onDelete={onDelete}
              onModify={onModify}
              onEdit={onEdit}
            />
          ))}
        </div>
        <div className="flex flex-col justify-start w-2xs gap-4">
          {part3.map((todo: Todo) => (
            <TodoItem
              key={todo.id}
              todo={todo}
              onDelete={onDelete}
              onModify={onModify}
              onEdit={onEdit}
            />
          ))}
        </div>
      </div>
    </div>
  );
}
