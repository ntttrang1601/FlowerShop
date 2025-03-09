using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuaHangHoa.Migrations
{
    /// <inheritdoc />
    public partial class delete_soft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PhieuXuats",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PhieuNhaps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TenNhanVien",
                table: "LienHes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PhieuXuats");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PhieuNhaps");

            migrationBuilder.DropColumn(
                name: "TenNhanVien",
                table: "LienHes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AspNetUsers");
        }
    }
}
