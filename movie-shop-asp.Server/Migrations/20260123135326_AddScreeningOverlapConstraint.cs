using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace movie_shop_asp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddScreeningOverlapConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. btree_gist 확장 기능 설치
            // (일반 컬럼(=)과 범위 컬럼(&&)을 섞어서 제약조건을 걸기 위해 필수입니다)
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            // 2. Exclusion Constraint 추가 SQL 실행
            // 해석: "ScreenId"가 같으면서(=) AND 시간 범위가 겹치면(&&) -> 막아라(EXCLUDE)
            // tsrange(시작, 종료, '[)'): 시작 시간 포함, 종료 시간 미포함 (14:00 종료, 14:00 시작 가능)
            migrationBuilder.Sql(@"
                ALTER TABLE ""Screening"".""Screens""
                ADD CONSTRAINT ""no_overlap_screening""
                EXCLUDE USING gist (
                    ""TheaterId"" WITH =,
                    tstzrange(""StartTime"", ""EndTime"", '[)') WITH &&
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 롤백 시 제약조건 삭제
            migrationBuilder.Sql(@"
                ALTER TABLE ""Screenings""
                DROP CONSTRAINT IF EXISTS ""no_overlap_screening"";
            ");
        }
    }
}
