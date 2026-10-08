using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class AppDbContext : DbContext
{
    // Constructor: Receives database connection & options from Program.cs and passes them to base DbContext
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Represents the 'Employees' table in the SQL Database
    public DbSet<Employee> Employees { get; set; }
}
