using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class company22 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "MacAddress",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "Processor",
                table: "Computers");

            migrationBuilder.RenameColumn(
                name: "StorageGB",
                table: "Computers",
                newName: "StorageSize");

            migrationBuilder.RenameColumn(
                name: "RamGB",
                table: "Computers",
                newName: "RamSpeed");

            migrationBuilder.RenameColumn(
                name: "PurchasePrice",
                table: "Computers",
                newName: "ProcessorSpeed");

            migrationBuilder.AlterColumn<string>(
                name: "SystemBarcode",
                table: "Supplies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Supplies",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Supplies",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Supplies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Supplies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Supplies",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Supplies",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Supplies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchaseDate",
                table: "Supplies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Supplies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "Supplies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Supplier",
                table: "Supplies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "Supplies",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WarrantyExpiry",
                table: "Supplies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Employees",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Employees",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Employees",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeNumber",
                table: "Employees",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Employees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "Computers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                table: "Computers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Computers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "Computers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AssetTag",
                table: "Computers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GraphicsCard",
                table: "Computers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProcessorCores",
                table: "Computers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessorName",
                table: "Computers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RamAmount",
                table: "Computers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RamType",
                table: "Computers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusBadgeClass",
                table: "Computers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusDisplayName",
                table: "Computers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Supplies_AssignedEmployeeId",
                table: "Supplies",
                column: "AssignedEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplies_CompanyId",
                table: "Supplies",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Computers_AssignedEmployeeId",
                table: "Computers",
                column: "AssignedEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Computers_CompanyId",
                table: "Computers",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Computers_Companies_CompanyId",
                table: "Computers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Computers_Employees_AssignedEmployeeId",
                table: "Computers",
                column: "AssignedEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Supplies_Companies_CompanyId",
                table: "Supplies",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Supplies_Employees_AssignedEmployeeId",
                table: "Supplies",
                column: "AssignedEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Computers_Companies_CompanyId",
                table: "Computers");

            migrationBuilder.DropForeignKey(
                name: "FK_Computers_Employees_AssignedEmployeeId",
                table: "Computers");

            migrationBuilder.DropForeignKey(
                name: "FK_Supplies_Companies_CompanyId",
                table: "Supplies");

            migrationBuilder.DropForeignKey(
                name: "FK_Supplies_Employees_AssignedEmployeeId",
                table: "Supplies");

            migrationBuilder.DropIndex(
                name: "IX_Supplies_AssignedEmployeeId",
                table: "Supplies");

            migrationBuilder.DropIndex(
                name: "IX_Supplies_CompanyId",
                table: "Supplies");

            migrationBuilder.DropIndex(
                name: "IX_Computers_AssignedEmployeeId",
                table: "Computers");

            migrationBuilder.DropIndex(
                name: "IX_Computers_CompanyId",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Supplies");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Supplies");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Supplies");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Supplies");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Supplies");

            migrationBuilder.DropColumn(
                name: "PurchaseDate",
                table: "Supplies");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Supplies");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "Supplies");

            migrationBuilder.DropColumn(
                name: "Supplier",
                table: "Supplies");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "Supplies");

            migrationBuilder.DropColumn(
                name: "WarrantyExpiry",
                table: "Supplies");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmployeeNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "GraphicsCard",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "ProcessorCores",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "ProcessorName",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "RamAmount",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "RamType",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "StatusBadgeClass",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "StatusDisplayName",
                table: "Computers");

            migrationBuilder.RenameColumn(
                name: "StorageSize",
                table: "Computers",
                newName: "StorageGB");

            migrationBuilder.RenameColumn(
                name: "RamSpeed",
                table: "Computers",
                newName: "RamGB");

            migrationBuilder.RenameColumn(
                name: "ProcessorSpeed",
                table: "Computers",
                newName: "PurchasePrice");

            migrationBuilder.AlterColumn<string>(
                name: "SystemBarcode",
                table: "Supplies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Supplies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Supplies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "Computers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "LastUpdatedBy",
                table: "Computers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Computers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "Computers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AssetTag",
                table: "Computers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

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
                name: "Notes",
                table: "Computers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Processor",
                table: "Computers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
