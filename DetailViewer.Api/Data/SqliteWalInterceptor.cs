using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DetailViewer.Api.Data
{
    public class SqliteWalInterceptor : DbConnectionInterceptor
    {
        /// <summary>
        /// Перехватывает событие открытия соединения и устанавливает режим WAL для SQLite.
        /// </summary>
        /// <param name="connection">Открытое соединение с базой данных.</param>
        /// <param name="eventData">Данные события окончания соединения.</param>
        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            SetWalMode(connection);
            base.ConnectionOpened(connection, eventData);
        }

        /// <summary>
        /// Асинхронно перехватывает событие открытия соединения и устанавливает режим WAL для SQLite.
        /// </summary>
        /// <param name="connection">Открытое соединение с базой данных.</param>
        /// <param name="eventData">Данные события окончания соединения.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public override Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            SetWalMode(connection);
            return base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
        }

        /// <summary>
        /// Устанавливает режим WAL (Write-Ahead Logging) для соединения SQLite.
        /// </summary>
        /// <param name="connection">Соединение с базой данных.</param>
        private static void SetWalMode(DbConnection connection)
        {
            if (connection is SqliteConnection sqliteConnection)
            {
                using (var command = sqliteConnection.CreateCommand())
                {
                    command.CommandText = "PRAGMA journal_mode=WAL;";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
