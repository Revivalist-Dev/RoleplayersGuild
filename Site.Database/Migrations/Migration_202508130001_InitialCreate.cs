using FluentMigrator;

namespace RoleplayersGuild.Site.Database.Migrations
{
    [Migration(202508130001, "Initial empty migration to establish baseline")]
    public class Migration_202508130001_InitialCreate : Migration
    {
        public override void Up()
        {
            // This migration is intentionally left empty.
            // Its purpose is to create the initial VersionInfo table in the database,
            // which FluentMigrator uses to track which migrations have been applied.
            // All existing schema objects are considered part of this baseline.
        }

        public override void Down()
        {
            // This migration cannot be rolled back.
        }
    }
}