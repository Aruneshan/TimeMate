using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeMate.Migrations
{
    /// <inheritdoc />
    public partial class Feed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "leaveRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "leaveRequests",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
