using Microsoft.EntityFrameworkCore;
using System.IO;

namespace DetailViewer.Api.Data
{
    public class ApplicationDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly string _connectionString;

        public ApplicationDbContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public ApplicationDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite(_connectionString).AddInterceptors(new SqliteWalInterceptor());
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
