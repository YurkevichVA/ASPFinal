using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASPFinal.Migrations
{
    /// <inheritdoc />
    public partial class NavUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddForeignKey(
            //    name: "FK_Items_Users_Id",
            //    table: "Items",
            //    column: "Id",
            //    principalTable: "Users",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Items_Users_Id",
            //    table: "Items");
        }
    }
}
