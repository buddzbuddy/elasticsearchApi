using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace elasticsearchApi.Data.Migrations
{
    [Migration("DbMigration")]
    [DbContext(typeof(ApiContext))]
    public class DbMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IIN = table.Column<string?>(type: "nvarchar(max)", nullable: true),
                    SIN = table.Column<string?>(type: "nvarchar(max)", nullable: true),
                    Last_Name = table.Column<string?>(type: "nvarchar(max)", nullable: true),
                    First_Name = table.Column<string?>(type: "nvarchar(max)", nullable: true),
                    Middle_Name = table.Column<string?>(type: "nvarchar(max)", nullable: true),
                    Date_of_Birth = table.Column<DateTime?>(type: "date", nullable: true),
                    Sex = table.Column<Guid?>(type: "uniqueidentifier", nullable: true),
                    PassportType = table.Column<Guid?>(type: "uniqueidentifier", nullable: true),
                    PassportSeries = table.Column<string?>(type: "nvarchar(50)", nullable: true),
                    PassportNo = table.Column<string?>(type: "nvarchar(max)", nullable: true),
                    Date_of_Issue = table.Column<DateTime?>(type: "date", nullable: true),
                    Issuing_Authority = table.Column<string?>(type: "nvarchar(max)", nullable: true),
                    FamilyState = table.Column<Guid?>(type: "uniqueidentifier", nullable: true),

                    // DB Computed Fields
                    CreatedAt = table.Column<DateTime>(type: "datetime", defaultValueSql: "getdate()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime", defaultValueSql: "getdate()"),
                    deleted = table.Column<bool>(type: "bit", defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Persons");
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
