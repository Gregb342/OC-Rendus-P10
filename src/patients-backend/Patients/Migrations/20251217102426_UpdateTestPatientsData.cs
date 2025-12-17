using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Patients.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTestPatientsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DateOfBirth", "PhoneNumber" },
                values: new object[] { new DateTime(1966, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "100-222-3333" });

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DateOfBirth", "PhoneNumber" },
                values: new object[] { new DateTime(1945, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "200-333-4444" });

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DateOfBirth", "PhoneNumber" },
                values: new object[] { new DateTime(2004, 6, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "300-444-5555" });

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DateOfBirth", "PhoneNumber" },
                values: new object[] { new DateTime(2002, 6, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "400-555-6666" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DateOfBirth", "PhoneNumber" },
                values: new object[] { new DateTime(1985, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "555-123-4567" });

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DateOfBirth", "PhoneNumber" },
                values: new object[] { new DateTime(1990, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "555-234-5678" });

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DateOfBirth", "PhoneNumber" },
                values: new object[] { new DateTime(1978, 9, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "555-345-6789" });

            migrationBuilder.UpdateData(
                table: "Patients",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DateOfBirth", "PhoneNumber" },
                values: new object[] { new DateTime(1995, 12, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "555-456-7890" });
        }
    }
}
