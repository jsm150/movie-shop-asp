using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace movie_shop_asp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddScreeningModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Screens",
                schema: "Screening",
                columns: table => new
                {
                    ScreenId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MovieId = table.Column<long>(type: "bigint", nullable: false),
                    TheaterId = table.Column<long>(type: "bigint", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SalesStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SalesEndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Screens", x => x.ScreenId);
                });

            migrationBuilder.CreateTable(
                name: "Theaters",
                schema: "Screening",
                columns: table => new
                {
                    TheaterId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Theaters", x => x.TheaterId);
                });

            migrationBuilder.CreateTable(
                name: "SeatHolds",
                schema: "Screening",
                columns: table => new
                {
                    ScreenId = table.Column<long>(type: "bigint", nullable: false),
                    SeatCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    HoldToken = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    HeldUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatHolds", x => new { x.ScreenId, x.SeatCode });
                    table.ForeignKey(
                        name: "FK_SeatHolds_Screens_ScreenId",
                        column: x => x.ScreenId,
                        principalSchema: "Screening",
                        principalTable: "Screens",
                        principalColumn: "ScreenId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TheaterSeats",
                schema: "Screening",
                columns: table => new
                {
                    TheaterId = table.Column<long>(type: "bigint", nullable: false),
                    SeatCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TheaterSeats", x => new { x.TheaterId, x.SeatCode });
                    table.ForeignKey(
                        name: "FK_TheaterSeats_Theaters_TheaterId",
                        column: x => x.TheaterId,
                        principalSchema: "Screening",
                        principalTable: "Theaters",
                        principalColumn: "TheaterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Screens_MovieId",
                schema: "Screening",
                table: "Screens",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_Screens_TheaterId",
                schema: "Screening",
                table: "Screens",
                column: "TheaterId");

            migrationBuilder.CreateIndex(
                name: "IX_SeatHolds_HoldToken",
                schema: "Screening",
                table: "SeatHolds",
                column: "HoldToken");

            migrationBuilder.CreateIndex(
                name: "IX_SeatHolds_ScreenId_Status",
                schema: "Screening",
                table: "SeatHolds",
                columns: new[] { "ScreenId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SeatHolds_Status_HeldUntil",
                schema: "Screening",
                table: "SeatHolds",
                columns: new[] { "Status", "HeldUntil" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeatHolds",
                schema: "Screening");

            migrationBuilder.DropTable(
                name: "TheaterSeats",
                schema: "Screening");

            migrationBuilder.DropTable(
                name: "Screens",
                schema: "Screening");

            migrationBuilder.DropTable(
                name: "Theaters",
                schema: "Screening");
        }
    }
}
