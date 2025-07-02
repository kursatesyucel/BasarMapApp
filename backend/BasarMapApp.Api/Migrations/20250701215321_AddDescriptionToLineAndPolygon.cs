using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BasarMapApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToLineAndPolygon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Polygons",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Lines",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Polygons");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Lines");
        }
    }
}
