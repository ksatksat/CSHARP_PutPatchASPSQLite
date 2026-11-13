using Microsoft.EntityFrameworkCore;
using SimplePutPatchApp.Data;
using SimplePutPatchApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// Create the SQLite database and seed a few items on first run
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.TodoItems.Any())
    {
        db.TodoItems.AddRange(
            new TodoItem { Title = "Learn HTTP PUT", Description = "PUT replaces the whole resource." },
            new TodoItem { Title = "Learn HTTP PATCH", Description = "PATCH changes only the fields you send." },
            new TodoItem { Title = "Build something cool", Description = "Use ASP.NET Core and SQLite." });
        db.SaveChanges();
    }
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
