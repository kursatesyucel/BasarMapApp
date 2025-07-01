using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace BasarMapApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLineAndPolygonGeometryTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Polygons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Polygon>(
                name: "Geometry",
                table: "Polygons",
                type: "geometry (polygon, 4326)",
                nullable: false,
                oldClrType: typeof(Polygon),
                oldType: "geometry");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Lines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<LineString>(
                name: "Geometry",
                table: "Lines",
                type: "geometry (linestring, 4326)",
                nullable: false,
                oldClrType: typeof(LineString),
                oldType: "geometry");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Polygons",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<Polygon>(
                name: "Geometry",
                table: "Polygons",
                type: "geometry",
                nullable: false,
                oldClrType: typeof(Polygon),
                oldType: "geometry (polygon, 4326)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Lines",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<LineString>(
                name: "Geometry",
                table: "Lines",
                type: "geometry",
                nullable: false,
                oldClrType: typeof(LineString),
                oldType: "geometry (linestring, 4326)");
        }
    }
}
