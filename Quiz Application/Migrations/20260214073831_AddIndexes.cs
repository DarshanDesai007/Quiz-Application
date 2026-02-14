using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quiz_Application.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_SessionId",
                table: "UserResponses",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Type",
                table: "Questions",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserResponses_SessionId",
                table: "UserResponses");

            migrationBuilder.DropIndex(
                name: "IX_Questions_Type",
                table: "Questions");
        }
    }
}
