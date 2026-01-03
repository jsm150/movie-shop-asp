using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Movie.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Movies",
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
                        principalTable: "Movies",
                        principalColumn: "MovieId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Actor");

            migrationBuilder.DropTable(
                name: "Movies");
        }
    }
}
