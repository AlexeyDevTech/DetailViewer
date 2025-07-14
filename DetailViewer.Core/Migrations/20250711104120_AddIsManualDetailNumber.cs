using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DetailViewer.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddIsManualDetailNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManualDetailNumber",
                table: "DocumentRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsManualDetailNumber",
                table: "DocumentRecords");
        }
    }
}
