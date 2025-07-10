using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DetailViewer.Core.Migrations
{
    /// <inheritdoc />
    public partial class MakeClassifierAnEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassNumber_Name",
                table: "ESKDNumbers");

            migrationBuilder.DropColumn(
                name: "ClassNumber_Number",
                table: "ESKDNumbers");

            migrationBuilder.AddColumn<int>(
                name: "ClassifierId",
                table: "ESKDNumbers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Classifiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Number = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classifiers_ESKDNumbers_Id",
                        column: x => x.Id,
                        principalTable: "ESKDNumbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Classifiers");

            migrationBuilder.DropColumn(
                name: "ClassifierId",
                table: "ESKDNumbers");

            migrationBuilder.AddColumn<string>(
                name: "ClassNumber_Name",
                table: "ESKDNumbers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClassNumber_Number",
                table: "ESKDNumbers",
                type: "INTEGER",
                nullable: true);
        }
    }
}
