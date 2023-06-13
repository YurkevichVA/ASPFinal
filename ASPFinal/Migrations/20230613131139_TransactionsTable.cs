using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASPFinal.Migrations
{
    /// <inheritdoc />
    public partial class TransactionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Items_Users_Id",
            //    table: "Items");

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ItemId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TransactionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => new { x.ItemId, x.UserId });
                    //table.ForeignKey(
                    //    name: "FK_Transactions_Users_ItemId",
                    //    column: x => x.ItemId,
                    //    principalTable: "Users",
                    //    principalColumn: "Id",
                    //    onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Items_Users_Id",
            //    table: "Items",
            //    column: "Id",
            //    principalTable: "Users",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }
    }
}
