using FluentMigrator;

namespace TrueCode.DbMigrations.Migrations;

[Migration(202610091000)]
public class M202610091000_CreateCurrencyTable : Migration
{
    public override void Up()
    {
        Create.Table("currency")
            .WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("rate").AsDecimal(18, 6).NotNullable();

        Create.Index("ux_currency_name")
            .OnTable("currency")
            .OnColumn("name").Ascending()
            .WithOptions().Unique();
    }

    public override void Down()
    {
        Delete.Table("currency");
    }
}
