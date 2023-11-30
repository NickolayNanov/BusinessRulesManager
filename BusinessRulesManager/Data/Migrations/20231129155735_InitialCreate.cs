using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessRulesManager.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessRuleDefinitions",
                columns: table => new
                {
                    IdBusinessRuleDefinition = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessRuleDefinitions", x => x.IdBusinessRuleDefinition);
                });

            migrationBuilder.CreateTable(
                name: "Conditions",
                columns: table => new
                {
                    ConditionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Operator = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogicalOperator = table.Column<int>(type: "int", nullable: false),
                    MinValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValuesList = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentConditionId = table.Column<int>(type: "int", nullable: true),
                    IdBusinessRuleDefinition = table.Column<int>(type: "int", nullable: false),
                    BusinessRuleDefinitionIdBusinessRuleDefinition = table.Column<int>(type: "int", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conditions", x => x.ConditionId);
                    table.ForeignKey(
                        name: "FK_Conditions_BusinessRuleDefinitions_BusinessRuleDefinitionIdBusinessRuleDefinition",
                        column: x => x.BusinessRuleDefinitionIdBusinessRuleDefinition,
                        principalTable: "BusinessRuleDefinitions",
                        principalColumn: "IdBusinessRuleDefinition");
                    table.ForeignKey(
                        name: "FK_Conditions_Conditions_ParentConditionId",
                        column: x => x.ParentConditionId,
                        principalTable: "Conditions",
                        principalColumn: "ConditionId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conditions_BusinessRuleDefinitionIdBusinessRuleDefinition",
                table: "Conditions",
                column: "BusinessRuleDefinitionIdBusinessRuleDefinition");

            migrationBuilder.CreateIndex(
                name: "IX_Conditions_ParentConditionId",
                table: "Conditions",
                column: "ParentConditionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Conditions");

            migrationBuilder.DropTable(
                name: "BusinessRuleDefinitions");
        }
    }
}
