using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessRulesManager.Migrations
{
    /// <inheritdoc />
    public partial class nestedconditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConditionId",
                table: "Conditions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNested",
                table: "Conditions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Conditions_ConditionId",
                table: "Conditions",
                column: "ConditionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conditions_Conditions_ConditionId",
                table: "Conditions",
                column: "ConditionId",
                principalTable: "Conditions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conditions_Conditions_ConditionId",
                table: "Conditions");

            migrationBuilder.DropIndex(
                name: "IX_Conditions_ConditionId",
                table: "Conditions");

            migrationBuilder.DropColumn(
                name: "ConditionId",
                table: "Conditions");

            migrationBuilder.DropColumn(
                name: "IsNested",
                table: "Conditions");
        }
    }
}
