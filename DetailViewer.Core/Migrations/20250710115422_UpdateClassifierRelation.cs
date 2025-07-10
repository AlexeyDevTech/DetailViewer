using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DetailViewer.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClassifierRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classifiers_ESKDNumbers_Id",
                table: "Classifiers");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Classifiers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Classifiers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ESKDNumbers_ClassifierId",
                table: "ESKDNumbers",
                column: "ClassifierId");

            migrationBuilder.AddForeignKey(
                name: "FK_ESKDNumbers_Classifiers_ClassifierId",
                table: "ESKDNumbers",
                column: "ClassifierId",
                principalTable: "Classifiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ESKDNumbers_Classifiers_ClassifierId",
                table: "ESKDNumbers");

            migrationBuilder.DropIndex(
                name: "IX_ESKDNumbers_ClassifierId",
                table: "ESKDNumbers");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Classifiers");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Classifiers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddForeignKey(
                name: "FK_Classifiers_ESKDNumbers_Id",
                table: "Classifiers",
                column: "Id",
                principalTable: "ESKDNumbers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
