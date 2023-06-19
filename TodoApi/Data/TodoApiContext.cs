using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public class TodoApiContext : DbContext
{
    public TodoApiContext(DbContextOptions<TodoApiContext> dbContextOptions) : base(dbContextOptions)
    {
        
    }
    public DbSet<User> User { get; set; }
    
    public DbSet<Todo> Todo { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Todo>()
            .Property(t => t.TimeCreated)
            .HasDefaultValueSql("now()");
    }


}