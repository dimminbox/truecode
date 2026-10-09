using FluentMigrator;

namespace TrueCode.DbMigrations.Migrations;

[Migration(202610091002)]
public class M202610091002_CreateUserAuthHistoryTable : Migration
{
    public override void Up()
    {
        Create.Table("user_auth_history")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("user_id").AsGuid().NotNullable()
                .ForeignKey("fk_user_auth_history_users", "users", "id")
            .WithColumn("event_type").AsString(32).NotNullable()
            .WithColumn("occurred_at").AsDateTimeOffset().NotNullable();

        Create.Index("ix_user_auth_history_user_id_occurred_at")
            .OnTable("user_auth_history")
            .OnColumn("user_id").Ascending()
            .OnColumn("occurred_at").Descending();
    }

    public override void Down()
    {
        Delete.Table("user_auth_history");
    }
}
