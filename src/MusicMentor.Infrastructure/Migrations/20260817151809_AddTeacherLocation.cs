using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicMentor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "TeacherProfiles",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "TeacherProfiles",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "TeacherProfiles");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "TeacherProfiles");
        }
    }
}
