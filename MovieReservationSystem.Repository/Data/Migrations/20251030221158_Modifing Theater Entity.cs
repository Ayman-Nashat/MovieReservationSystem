using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieReservationSystem.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModifingTheaterEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Theaters",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Theaters",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Columns",
                table: "Theaters",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Theaters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ExternalId",
                table: "Theaters",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Theaters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Theaters",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rows",
                table: "Theaters",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TheaterType",
                table: "Theaters",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Theaters",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Theaters_Name_Location",
                table: "Theaters",
                columns: new[] { "Name", "Location" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Theaters_Name_Location",
                table: "Theaters");

            migrationBuilder.DropColumn(
                name: "Columns",
                table: "Theaters");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Theaters");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Theaters");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Theaters");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Theaters");

            migrationBuilder.DropColumn(
                name: "Rows",
                table: "Theaters");

            migrationBuilder.DropColumn(
                name: "TheaterType",
                table: "Theaters");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Theaters");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Theaters",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Theaters",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
