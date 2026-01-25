using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace movie_shop_asp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemaTheaterModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Theater");

            migrationBuilder.RenameTable(
                name: "TheaterSeats",
                newName: "TheaterSeats",
                newSchema: "Theater");

            migrationBuilder.RenameTable(
                name: "Theaters",
                newName: "Theaters",
                newSchema: "Theater");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "TheaterSeats",
                schema: "Theater",
                newName: "TheaterSeats");

            migrationBuilder.RenameTable(
                name: "Theaters",
                schema: "Theater",
                newName: "Theaters");
        }
    }
}
