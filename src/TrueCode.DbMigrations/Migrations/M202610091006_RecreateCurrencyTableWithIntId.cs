using FluentMigrator;

namespace TrueCode.DbMigrations.Migrations;

[Migration(202610091006)]
public class M202610091006_RecreateCurrencyTableWithIntId : Migration
{
    public override void Up()
    {
        Delete.Table("currency");

        Create.Table("currency")
            .WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("code").AsString(3).NotNullable()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("rate").AsDecimal(18, 6).NotNullable()
            .WithColumn("updated_at").AsDateTimeOffset().NotNullable();

        Create.Index("ux_currency_code")
            .OnTable("currency")
            .OnColumn("code").Ascending()
            .WithOptions().Unique();

        Create.Index("ux_currency_name")
            .OnTable("currency")
            .OnColumn("name").Ascending()
            .WithOptions().Unique();
    }

    public override void Down()
    {
        Delete.Table("currency");

        Create.Table("currency")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("code").AsString(3).NotNullable()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("rate").AsDecimal(18, 6).NotNullable()
            .WithColumn("updated_at").AsDateTimeOffset().NotNullable();

        Create.Index("ux_currency_code")
            .OnTable("currency")
            .OnColumn("code").Ascending()
            .WithOptions().Unique();

        Create.Index("ux_currency_name")
            .OnTable("currency")
            .OnColumn("name").Ascending()
            .WithOptions().Unique();
    }
}
