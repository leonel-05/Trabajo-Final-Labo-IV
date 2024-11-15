using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolicitudMovimientoAppWeb_MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddLugares : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LugarDestinoId",
                table: "Movimientos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LugarSalidaId",
                table: "Movimientos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Lugares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lugares", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_LugarDestinoId",
                table: "Movimientos",
                column: "LugarDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_LugarSalidaId",
                table: "Movimientos",
                column: "LugarSalidaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movimientos_Lugares_LugarDestinoId",
                table: "Movimientos",
                column: "LugarDestinoId",
                principalTable: "Lugares",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Movimientos_Lugares_LugarSalidaId",
                table: "Movimientos",
                column: "LugarSalidaId",
                principalTable: "Lugares",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movimientos_Lugares_LugarDestinoId",
                table: "Movimientos");

            migrationBuilder.DropForeignKey(
                name: "FK_Movimientos_Lugares_LugarSalidaId",
                table: "Movimientos");

            migrationBuilder.DropTable(
                name: "Lugares");

            migrationBuilder.DropIndex(
                name: "IX_Movimientos_LugarDestinoId",
                table: "Movimientos");

            migrationBuilder.DropIndex(
                name: "IX_Movimientos_LugarSalidaId",
                table: "Movimientos");

            migrationBuilder.DropColumn(
                name: "LugarDestinoId",
                table: "Movimientos");

            migrationBuilder.DropColumn(
                name: "LugarSalidaId",
                table: "Movimientos");
        }
    }
}
