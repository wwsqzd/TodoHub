import { useState } from "react";
import { useCreateTodo } from "@/hooks/useCreateTodo";
import ButtonUI from "../ui/ButtonUI";
interface Todo {
    id: string;
    title: string;
    description: string;
    createdDate: string;
}

type Props = {
    isOpen: boolean;
    onClose: () => void;
    onSuccess?: (todo: Todo) => void;
};

export default function CreateTodoModalWindow({isOpen, onClose, onSuccess} : Props) 
{
    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");
    const { create, loading, error } = useCreateTodo();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
        const res = await create({title, description});
        if (res)
        {
            onSuccess?.(res);
            onClose();     
        }
    } catch (err: unknown)
    {
        console.log(err);
    }
  };
    if (!isOpen) return null;
    return (
        <div className="fixed inset-0 flex items-center justify-center z-50">
            <div className="absolute inset-0 bg-black/30 backdrop-blur-sm transition-opacity"></div>
            <div className="relative bg-white rounded-lg shadow-lg p-8 w-full max-w-md">
            <h2 className="text-2xl font-bold mb-4 text-blue-600">New Todo</h2>
            <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                <input
                type="text"
                placeholder="Title"
                required
                value={title}
                onChange={e => setTitle(e.target.value)}
                className="border rounded px-3 py-2"
                />
                <textarea
                placeholder="Description"
                required
                value={description}
                onChange={e => setDescription(e.target.value)}
                className="border rounded px-3 py-2 resize-none"
                rows={4}
                />
                <div className="text-sm text-gray-600 text-center">
                    {error && <p className="text-red-500 mb-2">{error}</p>}
                </div>
                <div className="flex gap-4 justify-end">
                    {/* <button
                        type="button"
                        className="px-4 py-2 rounded bg-gray-200 hover:bg-gray-300 cursor-pointer"
                        onClick={onClose}
                        disabled={loading}
                    >
                        Cancel
                    </button> */}
                    <ButtonUI color="light_gray" w="w-20" h="h-11" text="Cancel" action={onClose} disabled={loading}/>
                    <ButtonUI type="submit" color="blue" w="w-20" h="h-11" text="Create" disabled={loading}/>
                </div>
            </form>
        </div>
  </div>
    )
}