using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace movie_shop_asp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueTheaterModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Theaters_Name",
                table: "Theaters",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Theaters_Name",
                table: "Theaters");
        }
    }
}
