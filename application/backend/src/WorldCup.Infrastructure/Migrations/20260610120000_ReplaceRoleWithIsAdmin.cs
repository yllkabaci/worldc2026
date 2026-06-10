using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorldCup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceRoleWithIsAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Add the new flag (defaults to non-admin for every existing row).
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            // 2) Preserve the existing role data: anyone who was 'Admin' becomes IsAdmin = true.
            //    Runs before the column drop so the values survive SQLite's table rebuild.
            migrationBuilder.Sql("UPDATE \"Users\" SET \"IsAdmin\" = 1 WHERE \"Role\" = 'Admin';");

            // 3) Drop the obsolete Role column (triggers a SQLite table rebuild that keeps IsAdmin).
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1) Re-add Role (defaults everyone to the non-admin role).
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "User");

            // 2) Map the flag back onto the role string.
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = 'Admin' WHERE \"IsAdmin\" = 1;");

            // 3) Drop the flag.
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");
        }
    }
}
