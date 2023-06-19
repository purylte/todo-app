using System.Net;
using System.Text.Json;
using TodoApi.Exceptions;

namespace TodoApi.Middlewares;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            String informativeErrorMessage;
            switch(error)
            {
                case ApiException e:
                    response.StatusCode = (int)e.StatusCode;
                    informativeErrorMessage = e.ErrorMessage;
                    break;
                
                case KeyNotFoundException _:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    informativeErrorMessage = "Entity not found"; 
                    break;
                
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    informativeErrorMessage = "Unexpected error";
                    await Console.Out.WriteLineAsync(error.Message);
                    break;
            }
            
            var result = JsonSerializer.Serialize(new { error = informativeErrorMessage });
            await response.WriteAsync(result);
        }
    }
}