using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArenaBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "StudentPayments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceivedByUserName",
                table: "StudentPayments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "CashTransactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "StudentPayments");

            migrationBuilder.DropColumn(
                name: "ReceivedByUserName",
                table: "StudentPayments");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "CashTransactions");
        }
    }
}
