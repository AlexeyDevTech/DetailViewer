using Microsoft.EntityFrameworkCore;
using System.IO;

namespace DetailViewer.Api.Data
{
    public class ApplicationDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ApplicationDbContextFactory"/>.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных.</param>
        public ApplicationDbContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Создает новый экземпляр <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <returns>Новый экземпляр <see cref="ApplicationDbContext"/>.</returns>
        public ApplicationDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite(_connectionString).AddInterceptors(new SqliteWalInterceptor());
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
