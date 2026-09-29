using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class add_missing_kca_columnFixture : MigrationTest<add_missing_kca_column>
    {
        private static bool HasKca(IDirectDataMapper db)
        {
            return db.Query<ColumnRow>("SELECT \"name\" AS \"Name\" FROM pragma_table_info('AuthorMetadata')")
                .Any(c => c.Name == "Kca");
        }

        [Test]
        public void should_add_the_column_when_another_fork_consumed_migration_41()
        {
            // Stand in for a database from a fork whose 041 did something else, so ours was
            // recorded as applied without ever adding the column.
            var db = WithMigrationTestDb(c =>
            {
                c.Delete.Column("Kca").FromTable("AuthorMetadata");
            });

            HasKca(db).Should().BeTrue();
        }

        [Test]
        public void should_do_nothing_when_the_column_is_already_there()
        {
            var db = WithMigrationTestDb(c => { });

            HasKca(db).Should().BeTrue();
        }

        [Test]
        public void should_let_an_author_be_inserted_afterwards()
        {
            // The reported symptom was an insert naming Kca, so exercise that rather than
            // trusting the column list alone.
            var db = WithMigrationTestDb(c =>
            {
                c.Delete.Column("Kca").FromTable("AuthorMetadata");
            });

            db.Query<ColumnRow>(
                "INSERT INTO \"AuthorMetadata\" " +
                "(\"ForeignAuthorId\", \"TitleSlug\", \"Name\", \"SortName\", \"NameLastFirst\", " +
                " \"SortNameLastFirst\", \"Aliases\", \"Status\", \"Images\", \"Links\", " +
                " \"Genres\", \"Ratings\", \"Kca\") " +
                "VALUES ('1', 'slug', 'Author', 'Author', 'Author', 'Author', '[]', 0, '[]', '[]', " +
                " '[]', '{}', '');" +
                "SELECT \"Name\" FROM \"AuthorMetadata\" WHERE \"ForeignAuthorId\" = '1'")
                .Single().Name.Should().Be("Author");
        }

        private class ColumnRow
        {
            public string Name { get; set; }
        }
    }
}
