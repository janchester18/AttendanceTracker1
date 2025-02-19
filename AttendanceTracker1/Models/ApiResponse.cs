namespace AttendanceTracker1.Models
{
    public class ApiResponse<T>
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public ApiResponse(string status, string message, T data)
        {
            Status = status;
            Message = message;
            Data = data;
        }

        public static ApiResponse<T> Success(T data, string message = "Request was successful")
        {
            return new ApiResponse<T>("SUCCESS", message, data);
        }

        public static ApiResponse<T> Failed(string message, T data = default)
        {
            return new ApiResponse<T>("FAILED", message, data);
        }
    }

}
