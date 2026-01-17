using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace movie_shop_asp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddScreeningMovieAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Screening");

            migrationBuilder.CreateTable(
                name: "Movies",
                schema: "Screening",
                columns: table => new
                {
                    MovieId = table.Column<long>(type: "bigint", nullable: false),
                    MovieStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies1", x => x.MovieId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Movies",
                schema: "Screening");
        }
    }
}
