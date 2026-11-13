using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimplePutPatchApp.Data;
using SimplePutPatchApp.Models;

namespace SimplePutPatchApp.Controllers;

// PUT: the client sends the COMPLETE resource.
public record TodoPutDto(string? Title, string? Description, bool IsDone);

// PATCH: every field is optional, only the fields that are sent are changed.
public record TodoPatchDto(string? Title, string? Description, bool? IsDone);

public class TodoController : Controller
{
    private readonly AppDbContext _db;

    public TodoController(AppDbContext db) => _db = db;

    private static object ToResponse(TodoItem t) =>
        new { id = t.Id, title = t.Title, description = t.Description, isDone = t.IsDone };

    // HTTP GET /todos -> list all items
    [HttpGet("/todos")]
    public async Task<IActionResult> List()
    {
        var items = await _db.TodoItems.AsNoTracking().OrderBy(t => t.Id).ToListAsync();
        return Json(items.Select(ToResponse));
    }

    // HTTP PUT /todos/{id} -> REPLACE the whole item
    // Any field missing from the request is reset (description -> "", isDone -> false).
    [HttpPut("/todos/{id:int}")]
    public async Task<IActionResult> Replace(int id, [FromBody] TodoPutDto? dto)
    {
        var title = dto?.Title?.Trim();
        if (string.IsNullOrEmpty(title))
            return BadRequest(new { error = "PUT requires a non-empty title." });

        var item = await _db.TodoItems.FindAsync(id);
        if (item is null)
            return NotFound(new { error = $"Item {id} not found." });

        item.Title = title;
        item.Description = dto!.Description?.Trim() ?? string.Empty;
        item.IsDone = dto.IsDone;

        await _db.SaveChangesAsync();
        return Json(ToResponse(item));
    }

    // HTTP PATCH /todos/{id} -> PARTIAL update, only the fields that were sent
    [HttpPatch("/todos/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TodoPatchDto? dto)
    {
        if (dto is null || (dto.Title is null && dto.Description is null && dto.IsDone is null))
            return BadRequest(new { error = "PATCH needs at least one field to change." });

        var item = await _db.TodoItems.FindAsync(id);
        if (item is null)
            return NotFound(new { error = $"Item {id} not found." });

        if (dto.Title is not null)
        {
            var title = dto.Title.Trim();
            if (title.Length == 0)
                return BadRequest(new { error = "Title cannot be empty." });
            item.Title = title;
        }

        if (dto.Description is not null) item.Description = dto.Description.Trim();
        if (dto.IsDone is not null) item.IsDone = dto.IsDone.Value;

        await _db.SaveChangesAsync();
        return Json(ToResponse(item));
    }
}
