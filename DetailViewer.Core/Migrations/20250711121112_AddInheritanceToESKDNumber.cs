using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DetailViewer.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddInheritanceToESKDNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ESKDNumbers_Classifiers_ClassifierId",
                table: "ESKDNumbers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Classifiers",
                table: "Classifiers");

            migrationBuilder.DropColumn(
                name: "AssemblyNumber",
                table: "DocumentRecords");

            migrationBuilder.DropColumn(
                name: "ProductNumber",
                table: "DocumentRecords");

            migrationBuilder.RenameTable(
                name: "Classifiers",
                newName: "Classifier");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "ESKDNumbers",
                type: "TEXT",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AssemblyNumberId",
                table: "DocumentRecords",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductNumberId",
                table: "DocumentRecords",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Classifier",
                table: "Classifier",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRecords_AssemblyNumberId",
                table: "DocumentRecords",
                column: "AssemblyNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRecords_ProductNumberId",
                table: "DocumentRecords",
                column: "ProductNumberId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentRecords_ESKDNumbers_AssemblyNumberId",
                table: "DocumentRecords",
                column: "AssemblyNumberId",
                principalTable: "ESKDNumbers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentRecords_ESKDNumbers_ProductNumberId",
                table: "DocumentRecords",
                column: "ProductNumberId",
                principalTable: "ESKDNumbers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ESKDNumbers_Classifier_ClassifierId",
                table: "ESKDNumbers",
                column: "ClassifierId",
                principalTable: "Classifier",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentRecords_ESKDNumbers_AssemblyNumberId",
                table: "DocumentRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentRecords_ESKDNumbers_ProductNumberId",
                table: "DocumentRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ESKDNumbers_Classifier_ClassifierId",
                table: "ESKDNumbers");

            migrationBuilder.DropIndex(
                name: "IX_DocumentRecords_AssemblyNumberId",
                table: "DocumentRecords");

            migrationBuilder.DropIndex(
                name: "IX_DocumentRecords_ProductNumberId",
                table: "DocumentRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Classifier",
                table: "Classifier");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "ESKDNumbers");

            migrationBuilder.DropColumn(
                name: "AssemblyNumberId",
                table: "DocumentRecords");

            migrationBuilder.DropColumn(
                name: "ProductNumberId",
                table: "DocumentRecords");

            migrationBuilder.RenameTable(
                name: "Classifier",
                newName: "Classifiers");

            migrationBuilder.AddColumn<string>(
                name: "AssemblyNumber",
                table: "DocumentRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductNumber",
                table: "DocumentRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Classifiers",
                table: "Classifiers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ESKDNumbers_Classifiers_ClassifierId",
                table: "ESKDNumbers",
                column: "ClassifierId",
                principalTable: "Classifiers",
                principalColumn: "Id");
        }
    }
}
