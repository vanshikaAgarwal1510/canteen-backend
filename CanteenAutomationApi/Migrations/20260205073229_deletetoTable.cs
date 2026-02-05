using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanteenAutomationApi.Migrations
{
    /// <inheritdoc />
    public partial class deletetoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MenuItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MenuCategories",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MenuCategories");
        }
    }
}
