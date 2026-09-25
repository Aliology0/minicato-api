using FluentValidation;
using System.Diagnostics;
using System.Text.Json;

namespace AlMostashar.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly bool _isDevelopment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _isDevelopment = environment.IsDevelopment();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
                _logger.LogError(ex, "Unhandled exception | TraceId: {TraceId} | {Method} {Path}",
                    traceId, context.Request.Method, context.Request.Path);
                await HandleExceptionAsync(context, ex, traceId);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, string traceId)
        {
            context.Response.ContentType = "application/json";

            var statusCode = exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.StatusCode = statusCode;

            var title = exception switch
            {
                ValidationException => "Validation Error",
                InvalidOperationException => "Invalid Operation",
                UnauthorizedAccessException => "Unauthorized",
                KeyNotFoundException => "Not Found",
                _ => "Server Error"
            };

            // Build the detail field
            object detail = exception switch
            {
                ValidationException validationEx => validationEx.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                }),
                // In production, hide raw message for 500s to avoid leaking internals
                _ when statusCode == StatusCodes.Status500InternalServerError && !_isDevelopment
                    => "An unexpected error occurred. Please try again later.",
                _ => exception.Message
            };

            // Build the response
            var response = new Dictionary<string, object?>
            {
                ["status"] = statusCode,
                ["title"] = title,
                ["detail"] = detail,
                ["traceId"] = traceId,
                ["timestamp"] = DateTime.UtcNow.ToString("o"),
                ["method"] = context.Request.Method,
                ["path"] = _isDevelopment
                    ? $"{context.Request.Path}{context.Request.QueryString}"
                    : context.Request.Path.Value
            };

            // Development-only: include debugging details
            if (_isDevelopment)
            {
                response["exceptionType"] = exception.GetType().FullName;
                response["stackTrace"] = exception.StackTrace;

                if (exception.InnerException is not null)
                {
                    response["innerException"] = new
                    {
                        type = exception.InnerException.GetType().FullName,
                        message = exception.InnerException.Message
                    };
                }
            }

            var result = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(result);
        }
    }
}
