

namespace TodoHub.Main.Core.Common
{
    // something like a response form to a request
    public class Result<T>
    {
        public bool Success { get; private set; }
        public string Error { get; private set; }
        public T Value { get; private set; }
        public string Message { get; private set; } = string.Empty;

        private Result() { }

        public static Result<T> Ok(T value, string? message = null) => new() { Success = true, Value = value, Message = message ?? string.Empty};
        public static Result<T> Fail(string error) => new() { Success = false, Error = error };

    }
}
