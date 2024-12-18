using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandevuTakipSistemi.Web.Migrations
{
    /// <inheritdoc />
    public partial class AppointmentTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Appointment",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Appointment");
        }
    }
}
