using Kt6UpdateApi.Models;
using Kt6UpdateApi.Services;

namespace Kt6UpdateApi.Middleware;

public class UpdateLoggingMiddleware(RequestDelegate next, ILogger<UpdateLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, UpdateLogStore logStore)
    {
        if (!HttpMethods.IsPut(context.Request.Method) || !context.Request.Path.StartsWithSegments("/api/users"))
        {
            await next(context);
            return;
        }

        var requestTime = DateTimeOffset.UtcNow;
        var statusCode = StatusCodes.Status500InternalServerError;
        try
        {
            await next(context);
            statusCode = context.Response.StatusCode;
        }
        finally
        {
            var entry = new UpdateLogEntry
            {
                RequestTime = requestTime,
                Method = context.Request.Method,
                Path = context.Request.Path,
                StatusCode = statusCode,
                Success = statusCode >= 200 && statusCode < 300
            };
            logger.LogInformation("Обновление {Method} {Path}, статус {StatusCode}", entry.Method, entry.Path, entry.StatusCode);
            try
            {
                logStore.Write(entry);
            }
            catch (IOException exception)
            {
                logger.LogError(exception, "Не удалось записать запрос в журнал");
            }
            catch (UnauthorizedAccessException exception)
            {
                logger.LogError(exception, "Нет доступа к файлу журнала");
            }
        }
    }
}
