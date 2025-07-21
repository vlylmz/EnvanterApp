using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumnsToComputers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Details",
                table: "Computers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PurchaseDate",
                table: "Software",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "Software",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Software",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Computers",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "Computers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Computers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AssetTag",
                table: "Computers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Computers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Computers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Computers",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Computers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "Computers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedDate",
                table: "Computers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Computers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MacAddress",
                table: "Computers",
                type: "nvarchar(17)",
                maxLength: 17,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Computers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Computers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatingSystem",
                table: "Computers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Processor",
                table: "Computers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchaseDate",
                table: "Computers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PurchasePrice",
                table: "Computers",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RamGB",
                table: "Computers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StorageGB",
                table: "Computers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StorageType",
                table: "Computers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WarrantyEndDate",
                table: "Computers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Software_AssignedEmployeeId",
                table: "Software",
                column: "AssignedEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Software_CompanyId",
                table: "Software",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CompanyId",
                table: "Employees",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Companies_CompanyId",
                table: "Employees",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Software_Companies_CompanyId",
                table: "Software",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Software_Employees_AssignedEmployeeId",
                table: "Software",
                column: "AssignedEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Companies_CompanyId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Software_Companies_CompanyId",
                table: "Software");

            migrationBuilder.DropForeignKey(
                name: "FK_Software_Employees_AssignedEmployeeId",
                table: "Software");

            migrationBuilder.DropIndex(
                name: "IX_Software_AssignedEmployeeId",
                table: "Software");

            migrationBuilder.DropIndex(
                name: "IX_Software_CompanyId",
                table: "Software");

            migrationBuilder.DropIndex(
                name: "IX_Employees_CompanyId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Software");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "LastUpdatedDate",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "MacAddress",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "OperatingSystem",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "Processor",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "PurchaseDate",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "PurchasePrice",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "RamGB",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "StorageGB",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "StorageType",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "WarrantyEndDate",
                table: "Computers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PurchaseDate",
                table: "Software",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "Software",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Computers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "Computers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Computers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "AssetTag",
                table: "Computers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "Computers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
