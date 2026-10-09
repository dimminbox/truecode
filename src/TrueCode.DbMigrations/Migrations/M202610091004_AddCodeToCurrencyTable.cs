using FluentMigrator;

namespace TrueCode.DbMigrations.Migrations;

[Migration(202610091004)]
public class M202610091004_AddCodeToCurrencyTable : Migration
{
    public override void Up()
    {
        Alter.Table("currency")
            .AddColumn("code").AsString(3).NotNullable().WithDefaultValue("");

        Execute.Sql("ALTER TABLE currency ALTER COLUMN code DROP DEFAULT");

        Create.Index("ux_currency_code")
            .OnTable("currency")
            .OnColumn("code").Ascending()
            .WithOptions().Unique();
    }

    public override void Down()
    {
        Delete.Index("ux_currency_code").OnTable("currency");
        Delete.Column("code").FromTable("currency");
    }
}
