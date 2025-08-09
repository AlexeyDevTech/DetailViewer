using DetailViewer.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace DetailViewer.Core.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>, IDbContextFactory<ApplicationDbContext>
    {
        private readonly ISettingsService _settingsService;

        public ApplicationDbContextFactory(ISettingsService settingsService = null)
        {
            _settingsService = settingsService;
        }

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            string connectionString;

            if (_settingsService != null)
            {
                var settings = _settingsService.LoadSettings();
                connectionString = $"Data Source={settings.LocalDatabasePath}";
            }
            else
            {
                connectionString = "Data Source=" + Path.Combine(Directory.GetCurrentDirectory(), "temp.db");
            }

            optionsBuilder.UseSqlite(connectionString);
            return new ApplicationDbContext(optionsBuilder.Options);
        }

        public ApplicationDbContext CreateDbContext()
        {
            return CreateDbContext(null);
        }

        public ApplicationDbContext CreateRemoteDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var settings = _settingsService.LoadSettings();
            var connectionString = $"Data Source={settings.DatabasePath};Cache=Shared;Journal Mode=WAL";
            optionsBuilder.UseSqlite(connectionString);
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}