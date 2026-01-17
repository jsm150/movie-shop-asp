using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace movie_shop_asp.Server.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Movie");

            migrationBuilder.CreateTable(
                name: "Movies",
                schema: "Movie",
                columns: table => new
                {
                    MovieId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MovieInfo_Title = table.Column<string>(type: "text", nullable: false),
                    MovieInfo_Director = table.Column<string>(type: "text", nullable: false),
                    MovieInfo_Genres = table.Column<string[]>(type: "text[]", nullable: false),
                    MovieInfo_RuntimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    MovieInfo_AdienceRating = table.Column<string>(type: "text", nullable: false),
                    MovieInfo_Synopsis = table.Column<string>(type: "text", nullable: false),
                    MovieInfo_ReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MovieStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.MovieId);
                });

            migrationBuilder.CreateTable(
                name: "Actor",
                schema: "Movie",
                columns: table => new
                {
                    MovieInfoMovieId = table.Column<long>(type: "bigint", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    National = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actor", x => new { x.MovieInfoMovieId, x.Id });
                    table.ForeignKey(
                        name: "FK_Actor_Movies_MovieInfoMovieId",
                        column: x => x.MovieInfoMovieId,
                        principalSchema: "Movie",
                        principalTable: "Movies",
                        principalColumn: "MovieId",
                        onDelete: ReferentialAction.Cascade);
                });

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
            migrationBuilder.DropTable(
                name: "Actor",
                schema: "Movie");

            migrationBuilder.DropTable(
                name: "Movies",
                schema: "Movie");
        }
    }
}
