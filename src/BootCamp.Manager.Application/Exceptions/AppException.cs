namespace BootCamp.Manager.Application.Exceptions;
// خطای قابل انتظارِ کسب‌وکار؛ لایه Application به ASP.NET وابسته نیست.
public class AppException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
