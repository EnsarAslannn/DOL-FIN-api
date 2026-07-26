using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddDataProtectionKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c89b788a-3642-47df-bc6c-13654b03517c",
                column: "ConcurrencyStamp",
                value: "78ce6600-2913-4f1f-808d-96e3a48df105");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e2d83ab9-2bb6-46b6-b8db-4e115fa016b2",
                column: "ConcurrencyStamp",
                value: "e111d4c0-9f2c-45d0-88cc-051d66c181b8");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c89b788a-3642-47df-bc6c-13654b03517c",
                column: "ConcurrencyStamp",
                value: "180dc409-91b3-4204-ac43-5c253ce4fad3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e2d83ab9-2bb6-46b6-b8db-4e115fa016b2",
                column: "ConcurrencyStamp",
                value: "39972adb-34ed-4c07-967d-9b238e640d87");
        }
    }
}
