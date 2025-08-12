using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DetailViewer.Core.Data
{
    public class SqliteWalInterceptor : DbConnectionInterceptor
    {
        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            SetWalMode(connection);
            base.ConnectionOpened(connection, eventData);
        }

        public override Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            SetWalMode(connection);
            return base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
        }

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