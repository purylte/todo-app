using System.Net;

namespace TodoApi.Exceptions;

public class ApiException : Exception
{
    
    public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.BadRequest;
    
    public required String ErrorMessage { get; init; }
}