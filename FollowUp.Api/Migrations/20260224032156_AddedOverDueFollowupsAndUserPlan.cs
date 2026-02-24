using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FollowUp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedOverDueFollowupsAndUserPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Plan",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plan",
                table: "Users");
        }
    }
}
