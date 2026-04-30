using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalBookingAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsEncryptedToMedicalRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEncrypted",
                table: "MedicalRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEncrypted",
                table: "MedicalRecords");
        }
    }
}