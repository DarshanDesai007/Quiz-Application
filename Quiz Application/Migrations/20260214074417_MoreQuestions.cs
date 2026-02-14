using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Quiz_Application.Migrations
{
    /// <inheritdoc />
    public partial class MoreQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "CorrectAnswer", "OrderNo", "Text", "Type" },
                values: new object[,]
                {
                    { 9, "21", 9, "What is the largest ocean on Earth?", "SingleChoice" },
                    { 10, "25,26,27", 10, "Which of these are JavaScript frameworks?", "MultipleChoice" },
                    { 11, null, 11, "What is the square root of 144?", "ShortAnswer" },
                    { 12, null, 12, "Enter your emergency contact number", "PhoneNumber" },
                    { 13, "31", 13, "Who painted the Mona Lisa?", "SingleChoice" },
                    { 14, "33,34,35", 14, "Which of these are database systems?", "MultipleChoice" },
                    { 15, null, 15, "What does CSS stand for?", "ShortAnswer" },
                    { 16, null, 16, "Describe the difference between SQL and NoSQL databases.", "LongAnswer" },
                    { 17, "39", 17, "What is the speed of light approximately?", "SingleChoice" },
                    { 18, null, 18, "Enter your alternate mobile number", "PhoneNumber" },
                    { 19, null, 19, "What are the advantages of using cloud computing?", "LongAnswer" },
                    { 20, null, 20, "Explain the MVC architecture pattern and its benefits.", "LongAnswer" }
                });

            migrationBuilder.InsertData(
                table: "QuestionOptions",
                columns: new[] { "Id", "QuestionId", "Text" },
                values: new object[,]
                {
                    { 18, 9, "Atlantic Ocean" },
                    { 19, 9, "Indian Ocean" },
                    { 20, 9, "Arctic Ocean" },
                    { 21, 9, "Pacific Ocean" },
                    { 22, 10, "Django" },
                    { 23, 10, "Laravel" },
                    { 24, 10, "Flask" },
                    { 25, 10, "React" },
                    { 26, 10, "Angular" },
                    { 27, 10, "Vue.js" },
                    { 28, 13, "Vincent van Gogh" },
                    { 29, 13, "Pablo Picasso" },
                    { 30, 13, "Michelangelo" },
                    { 31, 13, "Leonardo da Vinci" },
                    { 32, 14, "Photoshop" },
                    { 33, 14, "MySQL" },
                    { 34, 14, "PostgreSQL" },
                    { 35, 14, "MongoDB" },
                    { 36, 14, "Excel" },
                    { 37, 17, "150,000 km/s" },
                    { 38, 17, "500,000 km/s" },
                    { 39, 17, "300,000 km/s" },
                    { 40, 17, "1,000,000 km/s" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "QuestionOptions",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 17);
        }
    }
}
