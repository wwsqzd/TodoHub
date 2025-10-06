export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen flex items-start justify-center bg-gray-50">
        <div className="w-full h-fit max-w-md p-8 mt-10 bg-white rounded shadow">
            {children}
        </div>
    </div>
  );
}