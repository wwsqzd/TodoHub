import { Users } from "@/types";
import DeleteButtonUI from "../ui/DeleteButtonUI";
import { useDeleteUser } from "@/hooks/useDeleteUser";

type Props = {
  user: Users;
  onDelete?: (id: string) => void;
};

export default function UserItem({ user, onDelete }: Props) {
  const { deleteUser } = useDeleteUser();

  const handleDelete = async (id: string) => {
    try {
      const res = await deleteUser(id);
      if (res) {
        onDelete?.(res);
      }
    } catch (err: unknown) {
      console.log(err);
    }
  };

  return (
    <div
      key={user.id}
      className="bg-white rounded-lg shadow-md p-6 flex justify-between border border-gray-200 hover:shadow-lg transition"
    >
      <div className="flex flex-col">
        <div className="font-bold text-xl mb-2 text-gray-900">{user.name}</div>
        <div className="text-gray-500 mb-1">{user.email}</div>
        <div
          className={`text-sm font-semibold mb-2 ${
            user.isAdmin ? "text-blue-600" : "text-green-600"
          }`}
        >
          {user.isAdmin ? "Admin" : "User"}
        </div>
        <div className="text-xs text-gray-400">
          Joined: {new Date(user.createdAt).toLocaleDateString()}
        </div>
      </div>
      <div className="flex items-end">
        <DeleteButtonUI handleDelete={() => handleDelete(user.id)} />
      </div>
    </div>
  );
}
