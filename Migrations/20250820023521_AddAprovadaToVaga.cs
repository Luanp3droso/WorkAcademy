using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkAcademy.Migrations
{
    /// <inheritdoc />
    public partial class AddAprovadaToVaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Aprovada",
                table: "Vagas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aprovada",
                table: "Vagas");
        }
    }
}
