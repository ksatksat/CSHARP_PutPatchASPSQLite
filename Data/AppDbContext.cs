using Microsoft.EntityFrameworkCore;
using SimplePutPatchApp.Models;

namespace SimplePutPatchApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
}
