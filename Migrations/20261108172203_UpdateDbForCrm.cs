using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstateApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDbForCrm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Agents_RealtorId",
                table: "Deals");

            migrationBuilder.AlterColumn<int>(
                name: "RealtorId",
                table: "Deals",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "Agents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Agents_RealtorId",
                table: "Deals",
                column: "RealtorId",
                principalTable: "Agents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Agents_RealtorId",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Agents");

            migrationBuilder.AlterColumn<int>(
                name: "RealtorId",
                table: "Deals",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Agents_RealtorId",
                table: "Deals",
                column: "RealtorId",
                principalTable: "Agents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
