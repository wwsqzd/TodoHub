import { Raleway } from "next/font/google";
import dynamic from "next/dynamic";
import StartPart from "@/components/features/StartPart";

const font = Raleway({
  subsets: ["latin"],
  weight: ["300", "400", "500", "700", "800"],
});

const DynamicBackEndPart = dynamic(
  () => import("@/components/features/BackEndPart"),
  {
    loading: () => <p>Loading...</p>,
  }
);

export default function Home() {
  return (
    <div className={font.className}>
      <div className="w-[100vw] min-h-[100vh] h-fit flex justify-start items-center flex-col bg-gray-50">
        <StartPart />
        <p className="text-2xl font-bold">
          Short documentation on implementation:
        </p>
        <DynamicBackEndPart />

        <div className="w-[1000px]">
          <p className="text-3xl">Front-End</p>
        </div>
      </div>
    </div>
  );
}
