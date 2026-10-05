using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanteenAutomationApi.Migrations
{
    /// <inheritdoc />
    public partial class changeDefaultMethodDeletable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsMethodDeletable",
                table: "PaymentMethods",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsMethodDeletable",
                table: "PaymentMethods",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }
    }
}
