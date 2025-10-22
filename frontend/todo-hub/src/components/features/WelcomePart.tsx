import ButtonUI from "../ui/ButtonUI";

type Props = {
  handleButton: () => void;
};

export default function WelcomePart({ handleButton }: Props) {
  return (
    <div className="w-full h-screen flex justify-center">
      <div className="flex flex-col items-center gap-5 mt-40">
        <h1 className="text-4xl font-bold">Welcome to your Dashboard!</h1>
        <p>Lets create a first to-do!</p>
        <ButtonUI
          color="blue"
          text="Create Todo"
          w="w-44"
          h="h-11"
          action={handleButton}
        />
      </div>
    </div>
  );
}
