const colorClasses: Record<string, string> = {
  blue: "bg-blue-500 hover:bg-blue-600",
  red: "bg-red-500 hover:bg-red-600",
  light_gray: "bg-gray-400 hover:bg-gray-500",
};

type Props = {
  color: keyof typeof colorClasses;
  text_color?: string;
  text: string;
  w: string;
  h: string;
  type?: "submit" | "reset" | "button";
  action?: () => void;
  disabled?: boolean;
};

export default function ButtonUI({
  color,
  text,
  w,
  h,
  action,
  disabled,
  text_color,
  type,
}: Props) {
  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && action && !disabled) {
      action();
    }
  };

  return (
    <button
      className={`
            ${w}
            ${h} 
            ${colorClasses[color]}
            text-white 
            ${text_color}
            font-semibold 
            py-2
            rounded
            transition
            cursor-pointer`}
      onClick={action}
      onKeyDown={handleKeyDown}
      disabled={disabled}
      type={type}
    >
      {text}
    </button>
  );
}
