using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InFerreteria.Migrations.Inventory
{
    /// <inheritdoc />
    public partial class Initial_Postgres_Identity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "articulo");

            migrationBuilder.DropTable(
                name: "error_log");

            migrationBuilder.DropTable(
                name: "categoria");

            migrationBuilder.DropTable(
                name: "proveedor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categoria",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "error_log",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    controller = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    form_json = table.Column<string>(type: "text", nullable: true),
                    level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    path = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    query_string = table.Column<string>(type: "text", nullable: true),
                    stack_trace = table.Column<string>(type: "text", nullable: true),
                    user_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_error_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proveedor",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    direccion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ruc = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proveedor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "articulo",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    categoria_id = table.Column<long>(type: "bigint", nullable: false),
                    proveedor_id = table.Column<long>(type: "bigint", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    codigo_barras = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    precio_compra = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    precio_venta = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    stock_actual = table.Column<int>(type: "integer", nullable: false),
                    stock_minimo = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_articulo", x => x.id);
                    table.ForeignKey(
                        name: "FK_articulo_categoria_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categoria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_articulo_proveedor_proveedor_id",
                        column: x => x.proveedor_id,
                        principalTable: "proveedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_articulo_categoria_id",
                table: "articulo",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_articulo_codigo",
                table: "articulo",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_articulo_proveedor_id",
                table: "articulo",
                column: "proveedor_id");

            migrationBuilder.CreateIndex(
                name: "IX_categoria_nombre",
                table: "categoria",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proveedor_nombre",
                table: "proveedor",
                column: "nombre");
        }
    }
}
