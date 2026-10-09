using FluentMigrator;

namespace TrueCode.DbMigrations.Migrations;

[Migration(202610091001)]
public class M202610091001_CreateUsersTable : Migration
{
    public override void Up()
    {
        Create.Table("users")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("login").AsString(50).NotNullable()
            .WithColumn("password_hash").AsString().NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable()
            .WithColumn("last_login_at").AsDateTimeOffset().Nullable()
            .WithColumn("refresh_token_hash").AsString().Nullable()
            .WithColumn("refresh_token_expires_at").AsDateTimeOffset().Nullable();

        Create.Index("ux_users_login")
            .OnTable("users")
            .OnColumn("login").Ascending()
            .WithOptions().Unique();

        Create.Index("ux_users_refresh_token_hash")
            .OnTable("users")
            .OnColumn("refresh_token_hash").Ascending()
            .WithOptions().Unique();
    }

    public override void Down()
    {
        Delete.Table("users");
    }
}
