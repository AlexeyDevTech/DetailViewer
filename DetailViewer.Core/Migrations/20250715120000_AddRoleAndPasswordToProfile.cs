using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DetailViewer.Core.Migrations
{
    public partial class AddRoleAndPasswordToProfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Profiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Profiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 2);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Profiles");
        }
    }
}
