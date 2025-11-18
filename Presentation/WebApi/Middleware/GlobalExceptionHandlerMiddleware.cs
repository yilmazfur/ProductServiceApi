using System.Net;
using System.Text.Json;
using FluentValidation;

namespace WebApplication1.Presentation.WebApi.Middleware
{
    /// <summary>
    /// Global exception handling middleware for consistent error responses
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, response) = exception switch
            {
                ValidationException validationEx => (
                    (int)HttpStatusCode.BadRequest,
                    (object)new
                    {
                        statusCode = (int)HttpStatusCode.BadRequest,
                        errors = validationEx.Errors.Select(e => e.ErrorMessage).ToList()
                    }
                ),
                KeyNotFoundException => (
                    (int)HttpStatusCode.NotFound,
                    new ErrorResponse
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = exception.Message
                    }
                ),
                InvalidOperationException => (
                    (int)HttpStatusCode.Conflict,
                    new ErrorResponse
                    {
                        StatusCode = (int)HttpStatusCode.Conflict,
                        Message = exception.Message
                    }
                ),
                ArgumentException => (
                    (int)HttpStatusCode.BadRequest,
                    new ErrorResponse
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Message = exception.Message
                    }
                ),
                _ => (
                    (int)HttpStatusCode.InternalServerError,
                    new ErrorResponse
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError,
                        Message = "An unexpected error occurred. Please try again later."
                    }
                )
            };

            context.Response.StatusCode = statusCode;

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
    }

    /// <summary>
    /// Extension method to register the global exception handler middleware
    /// </summary>
    public static class GlobalExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        }
    }
}
