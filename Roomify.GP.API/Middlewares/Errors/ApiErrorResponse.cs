namespace Roomify.GP.API.Middlewares.Errors
{
    public class ApiErrorResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public List<string>? Errors { get; set; }

        public ApiErrorResponse(int status, string message, List<string>? errors = null)
        {
            Status = status;
            Message = message;
            Errors = errors;
        }
    }
}
