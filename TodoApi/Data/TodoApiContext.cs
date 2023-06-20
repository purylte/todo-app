using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public class TodoApiContext : DbContext
{
    public TodoApiContext(DbContextOptions<TodoApiContext> dbContextOptions) : base(dbContextOptions)
    {
        
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Todo> Todos { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}