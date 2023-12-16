using System.Net;
using System.Text.Json;
using FluentValidation;

namespace TodoApi.Shared;

public class ValidationMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (ex is ValidationException validationException)
            {
                HttpResponse response = context.Response;
                response.ContentType = "application/json";
                response.StatusCode = (int)HttpStatusCode.BadRequest;

                await response.WriteAsync(JsonSerializer.Serialize(new
                {
                    Errors = validationException.Errors.Select(e => e.ErrorMessage)
                }));
            }
            else
            {
                throw;
            }
        }
    }
}
