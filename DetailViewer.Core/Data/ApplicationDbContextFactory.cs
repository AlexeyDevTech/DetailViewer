using Microsoft.EntityFrameworkCore;
using DetailViewer.Core.Interfaces;
using System.Threading.Tasks;

namespace DetailViewer.Core.Data
{
    public class ApplicationDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly ISettingsService _settingsService;

        public ApplicationDbContextFactory(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public ApplicationDbContext CreateDbContext()
        {
            var settings = _settingsService.LoadSettings();
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite($"Data Source={settings.LocalDatabasePath}");

            return new ApplicationDbContext(optionsBuilder.Options);
        }

        public Task<ApplicationDbContext> CreateDbContextAsync()
        {
            return Task.FromResult(CreateDbContext());
        }
    }
}
