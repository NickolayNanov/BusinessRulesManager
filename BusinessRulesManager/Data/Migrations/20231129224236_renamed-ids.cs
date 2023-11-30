using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessRulesManager.Migrations
{
    /// <inheritdoc />
    public partial class renamedids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conditions_BusinessRuleDefinitions_BusinessRuleDefinitionIdBusinessRuleDefinition",
                table: "Conditions");

            migrationBuilder.RenameColumn(
                name: "BusinessRuleDefinitionIdBusinessRuleDefinition",
                table: "Conditions",
                newName: "BusinessRuleDefinitionId");

            migrationBuilder.RenameColumn(
                name: "ConditionId",
                table: "Conditions",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Conditions_BusinessRuleDefinitionIdBusinessRuleDefinition",
                table: "Conditions",
                newName: "IX_Conditions_BusinessRuleDefinitionId");

            migrationBuilder.RenameColumn(
                name: "IdBusinessRuleDefinition",
                table: "BusinessRuleDefinitions",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "BusinessRuleDefinitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Conditions_BusinessRuleDefinitions_BusinessRuleDefinitionId",
                table: "Conditions",
                column: "BusinessRuleDefinitionId",
                principalTable: "BusinessRuleDefinitions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conditions_BusinessRuleDefinitions_BusinessRuleDefinitionId",
                table: "Conditions");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "BusinessRuleDefinitions");

            migrationBuilder.RenameColumn(
                name: "BusinessRuleDefinitionId",
                table: "Conditions",
                newName: "BusinessRuleDefinitionIdBusinessRuleDefinition");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Conditions",
                newName: "ConditionId");

            migrationBuilder.RenameIndex(
                name: "IX_Conditions_BusinessRuleDefinitionId",
                table: "Conditions",
                newName: "IX_Conditions_BusinessRuleDefinitionIdBusinessRuleDefinition");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "BusinessRuleDefinitions",
                newName: "IdBusinessRuleDefinition");

            migrationBuilder.AddForeignKey(
                name: "FK_Conditions_BusinessRuleDefinitions_BusinessRuleDefinitionIdBusinessRuleDefinition",
                table: "Conditions",
                column: "BusinessRuleDefinitionIdBusinessRuleDefinition",
                principalTable: "BusinessRuleDefinitions",
                principalColumn: "IdBusinessRuleDefinition");
        }
    }
}
