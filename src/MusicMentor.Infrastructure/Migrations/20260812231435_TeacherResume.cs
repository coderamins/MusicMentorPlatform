using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicMentor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TeacherResume : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "TeacherProfiles");

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "TeacherProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "TeacherProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResumeContentType",
                table: "TeacherProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResumeFileName",
                table: "TeacherProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResumeStoragePath",
                table: "TeacherProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResumeUploadedAtUtc",
                table: "TeacherProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAtUtc",
                table: "TeacherProfiles",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "TeacherProfiles");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "TeacherProfiles");

            migrationBuilder.DropColumn(
                name: "ResumeContentType",
                table: "TeacherProfiles");

            migrationBuilder.DropColumn(
                name: "ResumeFileName",
                table: "TeacherProfiles");

            migrationBuilder.DropColumn(
                name: "ResumeStoragePath",
                table: "TeacherProfiles");

            migrationBuilder.DropColumn(
                name: "ResumeUploadedAtUtc",
                table: "TeacherProfiles");

            migrationBuilder.DropColumn(
                name: "ReviewedAtUtc",
                table: "TeacherProfiles");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "TeacherProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
