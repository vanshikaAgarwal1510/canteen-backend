using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanteenAutomationApi.Migrations
{
    /// <inheritdoc />
    public partial class changeOrderStatusToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Orders"
                ALTER COLUMN "Status" TYPE integer
                USING (
                    CASE "Status"
                        WHEN 'Pending' THEN 1
                        WHEN 'Preparing' THEN 2
                        WHEN 'Ready' THEN 3
                        WHEN 'Completed' THEN 4
                        ELSE 1
                    END
                );
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Orders"
                ALTER COLUMN "Status" TYPE text
                USING (
                    CASE "Status"
                        WHEN 1 THEN 'Pending'
                        WHEN 2 THEN 'Preparing'
                        WHEN 3 THEN 'Ready'
                        WHEN 4 THEN 'Completed'
                        ELSE 'Pending'
                    END
                );
            """);
        }
    }
}