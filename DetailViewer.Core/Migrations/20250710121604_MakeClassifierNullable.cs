using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DetailViewer.Core.Migrations
{
    /// <inheritdoc />
    public partial class MakeClassifierNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ESKDNumbers_Classifiers_ClassifierId",
                table: "ESKDNumbers");

            migrationBuilder.AlterColumn<int>(
                name: "ClassifierId",
                table: "ESKDNumbers",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_ESKDNumbers_Classifiers_ClassifierId",
                table: "ESKDNumbers",
                column: "ClassifierId",
                principalTable: "Classifiers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ESKDNumbers_Classifiers_ClassifierId",
                table: "ESKDNumbers");

            migrationBuilder.AlterColumn<int>(
                name: "ClassifierId",
                table: "ESKDNumbers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ESKDNumbers_Classifiers_ClassifierId",
                table: "ESKDNumbers",
                column: "ClassifierId",
                principalTable: "Classifiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
