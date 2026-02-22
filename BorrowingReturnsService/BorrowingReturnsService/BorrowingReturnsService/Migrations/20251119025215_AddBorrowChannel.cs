using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BorrowingReturnsService.Migrations
{
    /// <inheritdoc />
    public partial class AddBorrowChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Channel",
                table: "Borrowings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Channel",
                table: "Borrowings");
        }
    }
}
