using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessRulesManager.Migrations
{
    /// <inheritdoc />
    public partial class AddedObjectType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ObjectType",
                table: "BusinessRuleDefinitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObjectType",
                table: "BusinessRuleDefinitions");
        }
    }
}
