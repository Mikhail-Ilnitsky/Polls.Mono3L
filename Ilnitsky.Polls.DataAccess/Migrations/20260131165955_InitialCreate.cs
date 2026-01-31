using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Ilnitsky.Polls.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Polls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(36)", nullable: false, collation: "ascii_general_ci"),
                    DateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Html = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Polls", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Respondents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(36)", nullable: false, collation: "ascii_general_ci"),
                    DateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Respondents", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(36)", nullable: false, collation: "ascii_general_ci"),
                    PollId = table.Column<Guid>(type: "UUID(36)", nullable: false, collation: "ascii_general_ci"),
                    Text = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AllowCustomAnswer = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowMultipleChoice = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    TargetAnswer = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MatchNextNumber = table.Column<int>(type: "int", nullable: true),
                    DefaultNextNumber = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RespondentSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(36)", nullable: false, collation: "ascii_general_ci"),
                    RespondentId = table.Column<Guid>(type: "UUID(36)", nullable: false, collation: "ascii_general_ci"),
                    DateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsMobile = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RemoteIpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserAgent = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AcceptLanguage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Platform = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Brand = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespondentSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RespondentSessions_Respondents_RespondentId",
                        column: x => x.RespondentId,
                        principalTable: "Respondents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(36)", nullable: false, collation: "ascii_general_ci"),
                    QuestionId = table.Column<Guid>(type: "UUID(36)", nullable: false, collation: "ascii_general_ci"),
                    Text = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Answers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RespondentAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(36)", nullable: false, collation: "ascii_general_ci"),
                    QuestionId = table.Column<Guid>(type: "UUID(36)", nullable: false, collation: "ascii_general_ci"),
                    PollId = table.Column<Guid>(type: "UUID(36)", nullable: false, collation: "ascii_general_ci"),
                    RespondentSessionId = table.Column<Guid>(type: "UUID(36)", nullable: false, collation: "ascii_general_ci"),
                    RespondentId = table.Column<Guid>(type: "UUID(36)", nullable: false, collation: "ascii_general_ci"),
                    DateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Text = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespondentAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RespondentAnswers_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RespondentAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RespondentAnswers_RespondentSessions_RespondentSessionId",
                        column: x => x.RespondentSessionId,
                        principalTable: "RespondentSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RespondentAnswers_Respondents_RespondentId",
                        column: x => x.RespondentId,
                        principalTable: "Respondents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionId",
                table: "Answers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_PollId",
                table: "Questions",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_RespondentAnswers_PollId",
                table: "RespondentAnswers",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_RespondentAnswers_QuestionId",
                table: "RespondentAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_RespondentAnswers_RespondentId",
                table: "RespondentAnswers",
                column: "RespondentId");

            migrationBuilder.CreateIndex(
                name: "IX_RespondentAnswers_RespondentSessionId",
                table: "RespondentAnswers",
                column: "RespondentSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RespondentSessions_RespondentId",
                table: "RespondentSessions",
                column: "RespondentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "RespondentAnswers");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "RespondentSessions");

            migrationBuilder.DropTable(
                name: "Polls");

            migrationBuilder.DropTable(
                name: "Respondents");
        }
    }
}
