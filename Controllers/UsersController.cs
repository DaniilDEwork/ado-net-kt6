using Kt6UpdateApi.Data;
using Kt6UpdateApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Kt6UpdateApi.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(UserDatabase database, ILogger<UsersController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            return Ok(await database.GetAllAsync());
        }
        catch (SqliteException exception)
        {
            return DatabaseError(exception);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var user = await database.GetByIdAsync(id);
            return user is null ? NotFound(new { message = "Пользователь не найден" }) : Ok(user);
        }
        catch (SqliteException exception)
        {
            return DatabaseError(exception);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        try
        {
            var user = await database.UpdateAsync(id, request);
            return user is null ? NotFound(new { message = "Пользователь не найден" }) : Ok(user);
        }
        catch (SqliteException exception)
        {
            return DatabaseError(exception);
        }
    }

    [HttpPut("{id:int}/email")]
    public async Task<IActionResult> UpdateEmail(int id, UpdateEmailRequest request)
    {
        try
        {
            var user = await database.UpdateEmailAsync(id, request);
            if (user is not null)
            {
                return Ok(user);
            }

            if (await database.GetByIdAsync(id) is null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            return Conflict(new { message = "Текущее имя не совпадает с переданным, почта не изменена" });
        }
        catch (SqliteException exception)
        {
            return DatabaseError(exception);
        }
    }

    [HttpPut("{id:int}/transaction")]
    public async Task<IActionResult> UpdateWithTransaction(int id, UpdateUserRequest request)
    {
        try
        {
            var user = await database.UpdateWithTransactionAsync(id, request);
            return user is null ? NotFound(new { message = "Пользователь не найден" }) : Ok(user);
        }
        catch (SqliteException exception)
        {
            return DatabaseError(exception);
        }
    }

    private ObjectResult DatabaseError(SqliteException exception)
    {
        logger.LogError(exception, "Ошибка базы данных при запросе {Method} {Path}", Request.Method, Request.Path);
        if (exception.SqliteExtendedErrorCode == 2067)
        {
            return Conflict(new { message = "Эта электронная почта уже используется другим пользователем" });
        }

        if (exception.SqliteErrorCode is 5 or 6 or 14)
        {
            return StatusCode(503, new { message = "База данных временно недоступна" });
        }

        return StatusCode(500, new { message = "Не удалось выполнить SQL-запрос" });
    }
}
