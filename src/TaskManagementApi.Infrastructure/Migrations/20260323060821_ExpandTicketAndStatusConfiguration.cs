using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TaskManagementApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandTicketAndStatusConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActualBehaviour",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualEndDate",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "Tickets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrdNumber",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Environment",
                table: "Tickets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpectedBehaviour",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedDueDate",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MilestoneId",
                table: "Tickets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PauseReason",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReporterId",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Severity",
                table: "Tickets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SprintId",
                table: "Tickets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StepsToReproduce",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StoryPoints",
                table: "Tickets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "Tickets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestCaseNumber",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TicketNumber",
                table: "Tickets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UseCaseNumber",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TicketAssignees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TicketId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketAssignees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketAssignees_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketAssignees_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketLabels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TicketId = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketLabels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketLabels_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceTicketId = table.Column<int>(type: "integer", nullable: false),
                    TargetTicketId = table.Column<int>(type: "integer", nullable: false),
                    LinkType = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketLinks_Tickets_SourceTicketId",
                        column: x => x.SourceTicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketLinks_Tickets_TargetTicketId",
                        column: x => x.TargetTicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Colour = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsTerminal = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketStatuses_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TicketId = table.Column<int>(type: "integer", nullable: false),
                    FromStatusId = table.Column<int>(type: "integer", nullable: false),
                    ToStatusId = table.Column<int>(type: "integer", nullable: false),
                    ChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketStatusHistories_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketStatusHistories_TicketStatuses_FromStatusId",
                        column: x => x.FromStatusId,
                        principalTable: "TicketStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketStatusHistories_TicketStatuses_ToStatusId",
                        column: x => x.ToStatusId,
                        principalTable: "TicketStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketStatusHistories_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ProductId",
                table: "Tickets",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ProjectId",
                table: "Tickets",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ReporterId",
                table: "Tickets",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_StatusId",
                table: "Tickets",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TeamId",
                table: "Tickets",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAssignees_TicketId",
                table: "TicketAssignees",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAssignees_UserId",
                table: "TicketAssignees",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLabels_TicketId",
                table: "TicketLabels",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLinks_SourceTicketId",
                table: "TicketLinks",
                column: "SourceTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLinks_TargetTicketId",
                table: "TicketLinks",
                column: "TargetTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStatuses_ProjectId",
                table: "TicketStatuses",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStatusHistories_ChangedByUserId",
                table: "TicketStatusHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStatusHistories_FromStatusId",
                table: "TicketStatusHistories",
                column: "FromStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStatusHistories_TicketId",
                table: "TicketStatusHistories",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStatusHistories_ToStatusId",
                table: "TicketStatusHistories",
                column: "ToStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AspNetUsers_ReporterId",
                table: "Tickets",
                column: "ReporterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Products_ProductId",
                table: "Tickets",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Projects_ProjectId",
                table: "Tickets",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Teams_TeamId",
                table: "Tickets",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_TicketStatuses_StatusId",
                table: "Tickets",
                column: "StatusId",
                principalTable: "TicketStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AspNetUsers_ReporterId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Products_ProductId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Projects_ProjectId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Teams_TeamId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_TicketStatuses_StatusId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "TicketAssignees");

            migrationBuilder.DropTable(
                name: "TicketLabels");

            migrationBuilder.DropTable(
                name: "TicketLinks");

            migrationBuilder.DropTable(
                name: "TicketStatusHistories");

            migrationBuilder.DropTable(
                name: "TicketStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ProductId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ProjectId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ReporterId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_StatusId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_TeamId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ActualBehaviour",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ActualEndDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "BrdNumber",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Environment",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ExpectedBehaviour",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ExpectedDueDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "MilestoneId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PauseReason",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ReporterId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SprintId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "StepsToReproduce",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "StoryPoints",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TestCaseNumber",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TicketNumber",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "UseCaseNumber",
                table: "Tickets");
        }
    }
}
