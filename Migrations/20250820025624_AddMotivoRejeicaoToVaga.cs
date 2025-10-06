using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkAcademy.Migrations
{
    /// <inheritdoc />
    public partial class AddMotivoRejeicaoToVaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MotivoRejeicao",
                table: "Vagas",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MotivoRejeicao",
                table: "Vagas");
        }
    }
}
