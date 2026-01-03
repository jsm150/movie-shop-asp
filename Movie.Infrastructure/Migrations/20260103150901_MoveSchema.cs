using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movie.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Movie");

            migrationBuilder.RenameTable(
                name: "Movies",
                newName: "Movies",
                newSchema: "Movie");

            migrationBuilder.RenameTable(
                name: "Actor",
                newName: "Actor",
                newSchema: "Movie");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Movies",
                schema: "Movie",
                newName: "Movies");

            migrationBuilder.RenameTable(
                name: "Actor",
                schema: "Movie",
                newName: "Actor");
        }
    }
}
