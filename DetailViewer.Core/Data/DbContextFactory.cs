using DetailViewer.Core.Data;
using Microsoft.EntityFrameworkCore;

public class DbContextFactory
{
    private readonly string _connectionString;

    public DbContextFactory(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public ApplicationDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite(_connectionString);
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
