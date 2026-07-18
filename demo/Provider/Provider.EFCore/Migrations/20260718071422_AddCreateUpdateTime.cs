using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Provider.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class AddCreateUpdateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateTime",
                table: "TestEntity",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "TestEntity",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateTime",
                table: "TestEntity");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "TestEntity");
        }
    }
}
