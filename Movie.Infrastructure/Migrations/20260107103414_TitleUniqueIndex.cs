using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movie.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TitleUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Movies_MovieInfo_Title",
                schema: "Movie",
                table: "Movies",
                column: "MovieInfo_Title",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Movies_MovieInfo_Title",
                schema: "Movie",
                table: "Movies");
        }
    }
}
