using Kt6UpdateApi.Data;
using Kt6UpdateApi.Middleware;
using Kt6UpdateApi.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<UserDatabase>();
builder.Services.AddSingleton<UpdateLogStore>();

var app = builder.Build();
await app.Services.GetRequiredService<UserDatabase>().InitializeAsync();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new { message = "Не удалось обработать запрос" });
}));
app.UseMiddleware<UpdateLoggingMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
await app.RunAsync();
