export interface Todo {
    id: string;
    title: string;
    description: string;
    createdDate: string;
    isCompleted: boolean;
}

export interface Users{
    id: string,
    name: string,
    email: string,
    isAdmin: boolean,
    createdAt: string
}

export interface Profile {
  name: string;
  email: string;
  IsAdmin: boolean;
  authProvider: string;
  pictureUrl: string;
  interface_language?: "en" | "de";
}