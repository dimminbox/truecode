using FluentMigrator;

namespace TrueCode.DbMigrations.Migrations;

[Migration(202610091003)]
public class M202610091003_RecreateCurrencyTableWithGuidId : Migration
{
    public override void Up()
    {
        Delete.Table("currency");

        Create.Table("currency")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
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

        Create.Table("currency")
            .WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("rate").AsDecimal(18, 6).NotNullable();

        Create.Index("ux_currency_name")
            .OnTable("currency")
            .OnColumn("name").Ascending()
            .WithOptions().Unique();
    }
}
