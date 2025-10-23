namespace UniversalLogParser.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;

        _logger.LogInformation("Incoming request: {method} {path}", request.Method, request.Path);

        await _next(context);

        _logger.LogInformation("Response status: {statusCode}", context.Response.StatusCode);
    }
}