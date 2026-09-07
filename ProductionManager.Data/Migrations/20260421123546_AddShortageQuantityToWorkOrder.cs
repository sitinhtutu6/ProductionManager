using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddShortageQuantityToWorkOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShortageQuantity",
                table: "WorkOrder",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortageQuantity",
                table: "WorkOrder");
        }
    }
}
