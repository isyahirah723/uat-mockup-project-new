using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UAT_System_API.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectToTestCycles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    run_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status_old = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status_new = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    crt_user_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dt_created = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "execution_attachments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    execution_step_id = table.Column<int>(type: "int", nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    file_path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_execution_attachments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "system_settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    system_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    language = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    timezone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email_enabled = table.Column<bool>(type: "bit", nullable: false),
                    smtp_host = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    smtp_port = table.Column<int>(type: "int", nullable: false),
                    smtp_user = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    smtp_password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    smtp_secure = table.Column<bool>(type: "bit", nullable: false),
                    from_email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    to_email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    log_user_activity = table.Column<bool>(type: "bit", nullable: false),
                    retention_period = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "test_assignments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    test_case_id = table.Column<int>(type: "int", nullable: false),
                    department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    team = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    assigned_by = table.Column<int>(type: "int", nullable: true),
                    assigned_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_assignments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "test_cases",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    test_case_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    test_department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    version_tag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    module = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cycle_id = table.Column<int>(type: "int", nullable: true),
                    priority = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approval_status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approved_by = table.Column<int>(type: "int", nullable: true),
                    approved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    assigned_to = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    test_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    test_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    execution_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    test_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dependencies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    conditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    control_notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_cases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "test_cycles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cycle_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dt_start = table.Column<DateTime>(type: "datetime2", nullable: true),
                    dt_end = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    auto_assign_rule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    module = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    project = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    priority = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    match_department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    assigned_to = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_cycles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "test_runs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    test_case_id = table.Column<int>(type: "int", nullable: false),
                    test_assignment_id = table.Column<int>(type: "int", nullable: true),
                    executed_by = table.Column<int>(type: "int", nullable: false),
                    run_status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_runs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    full_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dark_mode = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "test_feedbacks",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    test_case_id = table.Column<int>(type: "int", nullable: false),
                    user_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    rating = table.Column<int>(type: "int", nullable: true),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_feedbacks", x => x.id);
                    table.ForeignKey(
                        name: "FK_test_feedbacks_test_cases_test_case_id",
                        column: x => x.test_case_id,
                        principalTable: "test_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "test_steps",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    test_case_id = table.Column<int>(type: "int", nullable: false),
                    step_order = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    expected = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    actual = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pass_fail = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_steps", x => x.id);
                    table.ForeignKey(
                        name: "FK_test_steps_test_cases_test_case_id",
                        column: x => x.test_case_id,
                        principalTable: "test_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "execution_steps",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    test_case_id = table.Column<int>(type: "int", nullable: false),
                    run_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    run_id_fk = table.Column<int>(type: "int", nullable: true),
                    step_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    required_role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    execution_status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sequence_order = table.Column<int>(type: "int", nullable: false),
                    has_defect = table.Column<bool>(type: "bit", nullable: false),
                    severity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ticket_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    actual_result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    crt_user_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dt_created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_execution_steps", x => x.id);
                    table.ForeignKey(
                        name: "FK_execution_steps_test_runs_run_id_fk",
                        column: x => x.run_id_fk,
                        principalTable: "test_runs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_execution_steps_run_id_fk",
                table: "execution_steps",
                column: "run_id_fk");

            migrationBuilder.CreateIndex(
                name: "IX_test_feedbacks_test_case_id",
                table: "test_feedbacks",
                column: "test_case_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_steps_test_case_id",
                table: "test_steps",
                column: "test_case_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "execution_attachments");

            migrationBuilder.DropTable(
                name: "execution_steps");

            migrationBuilder.DropTable(
                name: "system_settings");

            migrationBuilder.DropTable(
                name: "test_assignments");

            migrationBuilder.DropTable(
                name: "test_cycles");

            migrationBuilder.DropTable(
                name: "test_feedbacks");

            migrationBuilder.DropTable(
                name: "test_steps");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "test_runs");

            migrationBuilder.DropTable(
                name: "test_cases");
        }
    }
}
