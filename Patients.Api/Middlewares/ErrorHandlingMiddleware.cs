using Patients.Application.Exceptions;
using Patients.Application.Services; 
using System.Net;
using System.Text.Json;

namespace Patients.Api.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger logger)
        {
            // Log del error
            logger.LogError(exception, "An unhandled exception occurred.");

            var response = context.Response;
            response.ContentType = "application/json";

            var statusCode = exception switch
            {
                DuplicatePatientException => (int)HttpStatusCode.Conflict,        // 409
                ConcurrencyException => (int)HttpStatusCode.Conflict,             // 409
                KeyNotFoundException => (int)HttpStatusCode.NotFound,             // 404
                ArgumentException => (int)HttpStatusCode.BadRequest,              // 400
                ValidationFailedException => (int)HttpStatusCode.BadRequest,      // 400
                _ => (int)HttpStatusCode.InternalServerError                     // 500
            };

            response.StatusCode = statusCode;

            object result;

            if (exception is ValidationFailedException validationEx)
            {
                result = new
                {
                    status = statusCode,
                    error = exception.GetType().Name,
                    message = exception.Message,
                    errors = validationEx.Errors, 
                    timestamp = DateTime.UtcNow
                };
            }
            else
            {
                result = new
                {
                    status = statusCode,
                    error = exception.GetType().Name,
                    message = exception.Message,
                    timestamp = DateTime.UtcNow
                };
            }

            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await response.WriteAsync(json);
        }

    }
}
