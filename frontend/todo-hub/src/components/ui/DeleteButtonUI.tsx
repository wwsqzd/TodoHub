import { BsTrash } from "react-icons/bs";

type Props = {
  handleDelete: () => void;
};

export default function DeleteButtonUI({ handleDelete }: Props) {
  return (
    <BsTrash
      onClick={handleDelete}
      className="cursor-pointer hover:text-red-700 w-4 h-4"
    />
  );
}
