using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DetailViewer.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ESKDNumber_CompanyCode = table.Column<string>(type: "TEXT", nullable: true),
                    ESKDNumber_ClassNumber_Name = table.Column<string>(type: "TEXT", nullable: true),
                    ESKDNumber_ClassNumber_Number = table.Column<int>(type: "INTEGER", nullable: true),
                    ESKDNumber_DetailNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    ESKDNumber_Version = table.Column<int>(type: "INTEGER", nullable: true),
                    YASTCode = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    AssemblyNumber = table.Column<string>(type: "TEXT", nullable: true),
                    AssemblyName = table.Column<string>(type: "TEXT", nullable: true),
                    ProductNumber = table.Column<string>(type: "TEXT", nullable: true),
                    ProductName = table.Column<string>(type: "TEXT", nullable: true),
                    FullName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentRecords");

            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
