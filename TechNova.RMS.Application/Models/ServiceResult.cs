using System.Net;

namespace TechNova.RMS.Models
{
    public class ServiceResult<T>
    {
        public HttpStatusCode? StatusCode { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }

        private ServiceResult() { }

        public static ServiceResult<T> Success(T data, HttpStatusCode statusCode = HttpStatusCode.OK, string message = null)
        {
            return new ServiceResult<T>
            {
                StatusCode = statusCode,
                Data = data,
                Message = message
            };
        }

        public static ServiceResult<T> Failure(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, T data = default(T))
        {
            return new ServiceResult<T>
            {
                StatusCode = statusCode,
                Data = data,
                Message = message
            };
        }
    }
}
