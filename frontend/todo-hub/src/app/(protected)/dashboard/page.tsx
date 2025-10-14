'use client'
import CreateTodoModalWindow  from "@/components/features/CreateTodoModalWindow";
import ButtonUI from "@/components/ui/ButtonUI";
import LoadingUI from "@/components/ui/LoadingUI";
import { getUserTodos, deleteTodo } from "@/lib/api";
import { useState, useEffect } from "react";
import { BsTrash } from "react-icons/bs";

export default function Dashboard() {
  const [todos, setTodos] = useState<Todo[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [loadingTodos, setLoadingTodos] = useState(false);

  interface Todo {
    id: string;
    title: string;
    description: string;
    createdDate: string;
  }

  useEffect(() => {
    setLoadingTodos(true);
    const getTodos = async () => {
      const res = await getUserTodos();
      setTodos(res.value);
      setLoadingTodos(false);
    };
    getTodos();
  }, []);

  const handleSuccess = (newTodo: Todo) => {
    setTodos(prev => [newTodo, ...prev]);
  };

  const handleButton = () => {
    setShowModal(true)
  }

  const handleDeleteTodo = async (id: string) => {
    try {
      const deletedTodoId = await deleteTodo(id);
      console.log(deletedTodoId);
      setTodos(prev => prev.filter(t => t.id !== deletedTodoId.value));
    } catch (err: unknown) {
      console.log(err);
    }
  }

  return (
    <>
      {showModal && (
        <CreateTodoModalWindow isOpen={showModal} onClose={() => setShowModal(false)} onSuccess={handleSuccess} />
      )}
  {loadingTodos ? (
    <LoadingUI />
) : todos.length > 0 ? (
  <div className="flex flex-col items-center">
  <ButtonUI color="blue" text="Create Todo" w="w-44" h="h-11" action={handleButton}/>
  <div className="min-h-screen flex flex-col items-center py-10">
    
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8 w-full max-w-5xl">
      {todos.map((todo: Todo) => (
        <div
          key={todo.id}
          className="bg-white rounded-lg shadow-md p-6 flex justify-between border border-gray-200 hover:shadow-lg transition"
        >
          <div className="flex flex-col items-start">
            <div className="font-bold text-xl mb-2 text-gray-900">{todo.title}</div>
            <div className="text-gray-500 mb-1">{todo.description}</div>
            <div className="text-xs text-gray-400">
              created: {new Date(todo.createdDate).toLocaleDateString()}
            </div>
          </div>
          <div className="h-max">
            <BsTrash onClick={() => handleDeleteTodo(todo.id)} className="cursor-pointer hover:text-red-600" />
          </div>
        </div>
      ))}
    </div>
  </div>
  </div>
) : (
  <div className="w-full h-screen flex justify-center">
    <div className="flex flex-col items-center gap-5 mt-40">
      <h1 className="text-4xl font-bold">Welcome to your Dashboard!</h1>
      <p>Lets create a first to-do!</p>
      <ButtonUI color="blue" text="Create Todo" w="w-44" h="h-11" action={handleButton}/>
    </div>
  </div>
)}
      
    </>
  );
}