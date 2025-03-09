using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuaHangHoa.Migrations
{
    /// <inheritdoc />
    public partial class ver1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PXSPTonKhos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TongSoLuong = table.Column<int>(type: "int", nullable: false),
                    NgayXuat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PXSPTonKhos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PXSPTonKhos_AspNetUsers_StaffId",
                        column: x => x.StaffId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CTPXSPTonKhos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PXSPTonKhoId = table.Column<int>(type: "int", nullable: false),
                    SanPhamId = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CTPXSPTonKhos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CTPXSPTonKhos_PXSPTonKhos_PXSPTonKhoId",
                        column: x => x.PXSPTonKhoId,
                        principalTable: "PXSPTonKhos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CTPXSPTonKhos_SanPhams_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CTPXSPTonKhos_PXSPTonKhoId",
                table: "CTPXSPTonKhos",
                column: "PXSPTonKhoId");

            migrationBuilder.CreateIndex(
                name: "IX_CTPXSPTonKhos_SanPhamId",
                table: "CTPXSPTonKhos",
                column: "SanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_PXSPTonKhos_StaffId",
                table: "PXSPTonKhos",
                column: "StaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CTPXSPTonKhos");

            migrationBuilder.DropTable(
                name: "PXSPTonKhos");
        }
    }
}
