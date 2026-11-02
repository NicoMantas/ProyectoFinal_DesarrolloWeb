namespace PeliculasAspnet.Exceptions;

public class ApiException : Exception
{
    public ApiException(int statusCode, string message, string error) : base(message)
    {
        StatusCode = statusCode;
        Error = error;
    }

    public int StatusCode { get; }

    public string Error { get; }
}
