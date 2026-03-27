using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalBookingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "Email", "FullName", "PasswordHash", "Phone", "Role" },
                values: new object[] { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "patient@medical.com", "Đỗ Minh Quân", "$2a$11$3swGMqzg1LEz7YIswcJ/geosBb6iBORBYrc5VYz25LnU.SKG.sktm", "0123456789", "Patient" });

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "PatientId", "DateOfBirth", "Gender", "UserId" },
                values: new object[] { 1, new DateTime(1990, 5, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Male", 3 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Patients",
                keyColumn: "PatientId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$RADz1ppX2rImcl5.FlJcle/OMibxH1kM6Sl8GIBd/EJukOPBbWh/i");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$8dsCsGqWIlvrnIKt220d4.1KMFXZOvd.pv317EDapE.ZNJ2JdQT.u");
        }
    }
}
