using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuaHangHoa.Migrations
{
    /// <inheritdoc />
    public partial class GiamGia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "PhanTramGiamGia",
                table: "SanPhams",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhanTramGiamGia",
                table: "SanPhams");
        }
    }
}
