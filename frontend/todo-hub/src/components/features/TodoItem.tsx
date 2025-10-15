import { BsTrash } from "react-icons/bs";
import { PiPencil } from "react-icons/pi";
import { MdOutlineCheckBoxOutlineBlank } from "react-icons/md";
import { MdOutlineCheckBox } from "react-icons/md";
import { useDeleteTodo } from "@/hooks/useDeleteTodo";
import { Todo } from "@/types";
import { useModifyTodo } from "@/hooks/useModifyTodo";

type Props = {
  todo: Todo;
  onDelete?: (id: string) => void;
  onModify?: (todo: Todo) => void;
  onEdit: (todo: Todo) => void;
};

export default function TodoItem({ todo, onDelete, onModify, onEdit }: Props) {
  const { delTodo } = useDeleteTodo();
  const { modify } = useModifyTodo();

  const handleDelete = async (id: string) => {
    try {
      const res = await delTodo(id);
      if (res) {
        onDelete?.(res);
      }
    } catch (err: unknown) {
      console.log(err);
    }
  };

  const handleModify = async (
    id: string,
    data: { title: string; description: string; isCompleted: boolean }
  ) => {
    try {
      const res = await modify(id, data);
      if (res) {
        onModify?.(res);
      }
    } catch (err: unknown) {
      console.log(err);
    }
  };

  return (
    <div
      key={todo.id}
      className="max-w-xs w-full h-fit bg-white rounded-lg shadow-md p-6 flex flex-col justify-between gap-3 border border-gray-200 hover:shadow-lg transition-all"
    >
      <div className="flex flex-col items-start gap-2">
        <p className="font-bold text-xl text-gray-900 break-words break-all">
          {todo.title}
        </p>
        <p className="text-gray-500 break-words break-all">
          {todo.description}
        </p>
        <p className="text-xs text-gray-400">
          created: {new Date(todo.createdDate).toLocaleDateString()}
        </p>
      </div>
      <div className="flex justify-between gap-3 items-center">
        <div>
          {todo.isCompleted ? (
            <MdOutlineCheckBox
              onClick={() =>
                handleModify(todo.id, {
                  title: todo.title,
                  description: todo.description,
                  isCompleted: false,
                })
              }
              className="cursor-pointer text-green-600  w-5 h-5"
            />
          ) : (
            <MdOutlineCheckBoxOutlineBlank
              onClick={() =>
                handleModify(todo.id, {
                  title: todo.title,
                  description: todo.description,
                  isCompleted: true,
                })
              }
              className="cursor-pointer hover:text-green-500 w-5 h-5"
            />
          )}
        </div>
        <div className="flex justify-center gap-2 items-center">
          <PiPencil
            onClick={() => onEdit(todo)}
            className="cursor-pointer hover:text-blue-600 w-5 h-5"
          />

          <BsTrash
            onClick={() => handleDelete(todo.id)}
            className="cursor-pointer hover:text-red-700 w-4 h-4"
          />
        </div>
      </div>
    </div>
  );
}
