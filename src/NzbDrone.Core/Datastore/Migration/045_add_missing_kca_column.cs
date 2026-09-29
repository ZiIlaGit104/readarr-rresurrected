using System.Data;
using System.Linq;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    /// <summary>
    /// Repairs databases carried over from another Readarr fork.
    ///
    /// FluentMigrator records only the number of a migration, never what it did, so a database
    /// that already ran some other fork's 041 carries 41 in VersionInfo and skips ours for good.
    /// Faustvii/Readarr numbers an index migration 041 where this fork adds AuthorMetadata.Kca,
    /// and both forks are identical through 040. A database moved across therefore reaches 044
    /// with no Kca column, and every author insert fails with
    /// "table AuthorMetadata has no column named Kca".
    ///
    /// Adding the column again under a fresh number is the only way back, since 41 can never be
    /// replayed. The check makes it a no-op for databases that did run our 041.
    /// </summary>
    [Migration(045)]
    public class add_missing_kca_column : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Both branches decide at execution time. Schema.Exists() would read the schema
            // while the expression list is still being built, which is the wrong moment to ask.
            IfDatabase("postgres").Execute.Sql(
                "ALTER TABLE \"AuthorMetadata\" ADD COLUMN IF NOT EXISTS \"Kca\" TEXT");

            IfDatabase("sqlite").Execute.WithConnection(AddKcaIfMissing);
        }

        protected override void CacheDbUpgrade()
        {
            // 041 purged the cache so responses predating Kca were refetched. That was skipped
            // on the same databases, so the stale entries are still there.
            Delete.FromTable("HttpResponse").AllRows();
        }

        private void AddKcaIfMissing(IDbConnection conn, IDbTransaction tran)
        {
            var columns = conn.Query<string>(
                "SELECT \"name\" FROM pragma_table_info('AuthorMetadata')",
                transaction: tran).ToList();

            if (columns.Contains("Kca"))
            {
                return;
            }

            _logger.Info("AuthorMetadata.Kca is missing, most likely because another fork's " +
                         "migration 41 was applied to this database. Adding it now.");

            conn.Execute("ALTER TABLE \"AuthorMetadata\" ADD COLUMN \"Kca\" TEXT", transaction: tran);
        }
    }
}
