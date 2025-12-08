import { Users } from "@/types";
import UserItem from "./UserItem";

type Props = {
  usersData: Array<Users>;
  onDelete?: (id: string) => void;
};

export default function UserList({ usersData, onDelete }: Props) {
  return (
    <div className="min-h-screen flex flex-col items-center py-10">
      <h2 className="text-3xl font-bold mb-8 text-blue-500">Users List</h2>
      <div className="sm:grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8 w-full max-w-5xl flex flex-col items-center">
        {usersData.map((user: Users) => (
          <UserItem key={user.id} user={user} onDelete={onDelete} />
        ))}
      </div>
    </div>
  );
}
