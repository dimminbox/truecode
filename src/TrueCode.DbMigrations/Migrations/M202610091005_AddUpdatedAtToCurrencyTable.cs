using FluentMigrator;

namespace TrueCode.DbMigrations.Migrations;

[Migration(202610091005)]
public class M202610091005_AddUpdatedAtToCurrencyTable : Migration
{
    public override void Up()
    {
        Alter.Table("currency")
            .AddColumn("updated_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTimeOffset);

        Execute.Sql("ALTER TABLE currency ALTER COLUMN updated_at DROP DEFAULT");
    }

    public override void Down()
    {
        Delete.Column("updated_at").FromTable("currency");
    }
}
