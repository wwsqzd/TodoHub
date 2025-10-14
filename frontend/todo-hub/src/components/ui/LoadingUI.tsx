import { PiSpinner } from "react-icons/pi";
export default function LoadingUI()
{
    return (
        <div className="w-full h-[calc(100vh-110px)] flex justify-center items-center">
            <PiSpinner className="animate-spin w-10 h-10"/>
        </div>
    )
}