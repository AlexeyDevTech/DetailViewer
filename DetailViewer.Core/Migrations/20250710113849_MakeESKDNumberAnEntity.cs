using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DetailViewer.Core.Migrations
{
    /// <inheritdoc />
    public partial class MakeESKDNumberAnEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ESKDNumber_ClassNumber_Name",
                table: "DocumentRecords");

            migrationBuilder.DropColumn(
                name: "ESKDNumber_ClassNumber_Number",
                table: "DocumentRecords");

            migrationBuilder.DropColumn(
                name: "ESKDNumber_CompanyCode",
                table: "DocumentRecords");

            migrationBuilder.DropColumn(
                name: "ESKDNumber_DetailNumber",
                table: "DocumentRecords");

            migrationBuilder.DropColumn(
                name: "ESKDNumber_Version",
                table: "DocumentRecords");

            migrationBuilder.AddColumn<int>(
                name: "ESKDNumberId",
                table: "DocumentRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ESKDNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    CompanyCode = table.Column<string>(type: "TEXT", nullable: true),
                    ClassNumber_Name = table.Column<string>(type: "TEXT", nullable: true),
                    ClassNumber_Number = table.Column<int>(type: "INTEGER", nullable: true),
                    DetailNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ESKDNumbers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRecords_ESKDNumberId",
                table: "DocumentRecords",
                column: "ESKDNumberId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentRecords_ESKDNumbers_ESKDNumberId",
                table: "DocumentRecords",
                column: "ESKDNumberId",
                principalTable: "ESKDNumbers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentRecords_ESKDNumbers_ESKDNumberId",
                table: "DocumentRecords");

            migrationBuilder.DropTable(
                name: "ESKDNumbers");

            migrationBuilder.DropIndex(
                name: "IX_DocumentRecords_ESKDNumberId",
                table: "DocumentRecords");

            migrationBuilder.DropColumn(
                name: "ESKDNumberId",
                table: "DocumentRecords");

            migrationBuilder.AddColumn<string>(
                name: "ESKDNumber_ClassNumber_Name",
                table: "DocumentRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ESKDNumber_ClassNumber_Number",
                table: "DocumentRecords",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ESKDNumber_CompanyCode",
                table: "DocumentRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ESKDNumber_DetailNumber",
                table: "DocumentRecords",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ESKDNumber_Version",
                table: "DocumentRecords",
                type: "INTEGER",
                nullable: true);
        }
    }
}
