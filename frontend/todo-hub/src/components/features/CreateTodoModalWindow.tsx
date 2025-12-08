import { useState } from "react";
import { useCreateTodo } from "@/hooks/useCreateTodo";
import ButtonUI from "../ui/ButtonUI";
import { Todo } from "@/types";
import { useLanguage } from "@/context/LanguageContext";
import { translations } from "@/lib/dictionary";

type Props = {
  isOpen: boolean;
  onClose: () => void;
  onCreate?: (todo: Todo) => void;
};

export default function CreateTodoModalWindow({
  isOpen,
  onClose,
  onCreate,
}: Props) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const { create, loading, error } = useCreateTodo();

  const { language } = useLanguage();
  const t = translations[language];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await create({ title, description });
      if (res) {
        onCreate?.(res);
        onClose();
      }
    } catch (err: unknown) {
      console.log(err);
    }
  };

  if (!isOpen) return null;
  return (
    <div className="fixed inset-0 flex items-center justify-center z-50">
      <div className="absolute inset-0 bg-black/30 backdrop-blur-sm transition-opacity"></div>
      <div className="relative bg-white rounded-lg shadow-lg p-8 sm:w-full w-[80vw] max-w-md">
        <h2 className="sm:text-2xl text-xl font-bold mb-4 text-blue-600">
          {t.create}
        </h2>
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <input
            type="text"
            placeholder={t.titlePlaceholder}
            required
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className="border rounded px-3 py-2"
          />
          <textarea
            placeholder={t.descriptionPlaceholder}
            required
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            className="border rounded px-3 py-2 resize-none"
            rows={4}
          />
          <div className="text-sm text-gray-600 text-center">
            {error && <p className="text-red-500 mb-2">{error}</p>}
          </div>
          <div className="flex gap-4 justify-end">
            <ButtonUI
              color="light_gray"
              w="w-20"
              h="h-11"
              text={t.cancel}
              action={onClose}
              disabled={loading}
            />
            <ButtonUI
              type="submit"
              color="blue"
              w="w-20"
              h="h-11"
              text={t.create}
              disabled={loading}
            />
          </div>
        </form>
      </div>
    </div>
  );
}
