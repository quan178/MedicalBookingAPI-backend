using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalBookingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPrescriptionToMedicalRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Prescription",
                table: "MedicalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Q9DFtMde0uOe0WoGKB5EGOM1XXRULd/dpMi7AE500sSuWZ949..OO");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$tqFgqaG5cyGDaS/YSnd3IOTboByZN2mbSZ91a22yEktWkj5Q7hbg.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$0Eksse0HzOW/8EbOMgr5zu1giwYYenwpxmF93hBSs3qpwctrqE3Dy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prescription",
                table: "MedicalRecords");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$jV/J93pl/tTLvTIgplRRcOiKuvFD4CmYF9x/5Ha9.TkYFkzS59Ubm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$U1Vf2Kbc9i1V08DDLzc0guTMxysxA4JN/5cW4mZ5Wh6dD5NrV2SC6");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$3swGMqzg1LEz7YIswcJ/geosBb6iBORBYrc5VYz25LnU.SKG.sktm");
        }
    }
}
