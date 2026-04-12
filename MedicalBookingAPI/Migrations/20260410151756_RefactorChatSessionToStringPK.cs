using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalBookingAPI.Migrations
{
    /// <inheritdoc />
    public partial class RefactorChatSessionToStringPK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bước 1: Xóa unique index trên SessionToken
            migrationBuilder.DropIndex(
                name: "IX_ChatSessions_SessionToken",
                table: "ChatSessions");

            // Bước 2: Xóa cột SessionToken (không còn dùng)
            migrationBuilder.DropColumn(
                name: "SessionToken",
                table: "ChatSessions");

            // Bước 3: Xóa FK constraint từ ChatMessages -> ChatSessions
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_ChatSessions_ChatSessionId",
                table: "ChatMessages");

            // Bước 4: Xóa index trên ChatMessages.ChatSessionId
            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_ChatSessionId",
                table: "ChatMessages");

            // Bước 5: Xóa cột ChatSessionId trong ChatMessages
            migrationBuilder.DropColumn(
                name: "ChatSessionId",
                table: "ChatMessages");

            // Bước 6: Xóa PK constraint trên ChatSessions.ChatSessionId (int)
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatSessions",
                table: "ChatSessions");

            // Bước 7: Xóa cột ChatSessionId (int) trong ChatSessions
            migrationBuilder.DropColumn(
                name: "ChatSessionId",
                table: "ChatSessions");

            // Bước 8: Thêm cột ChatSessionId (string) mới trong ChatSessions
            migrationBuilder.AddColumn<string>(
                name: "ChatSessionId",
                table: "ChatSessions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            // Bước 9: Tạo PK mới trên ChatSessionId (string)
            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatSessions",
                table: "ChatSessions",
                column: "ChatSessionId");

            // Bước 10: Thêm cột ChatSessionId (string) trong ChatMessages
            migrationBuilder.AddColumn<string>(
                name: "ChatSessionId",
                table: "ChatMessages",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            // Bước 11: Tạo index trên ChatMessages.ChatSessionId
            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ChatSessionId",
                table: "ChatMessages",
                column: "ChatSessionId");

            // Bước 12: Tạo FK constraint từ ChatMessages -> ChatSessions
            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_ChatSessions_ChatSessionId",
                table: "ChatMessages",
                column: "ChatSessionId",
                principalTable: "ChatSessions",
                principalColumn: "ChatSessionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: Xóa FK và re-create các cột int

            // Bước 1: Xóa FK constraint
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_ChatSessions_ChatSessionId",
                table: "ChatMessages");

            // Bước 2: Xóa index
            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_ChatSessionId",
                table: "ChatMessages");

            // Bước 3: Xóa cột ChatSessionId (string) trong ChatMessages
            migrationBuilder.DropColumn(
                name: "ChatSessionId",
                table: "ChatMessages");

            // Bước 4: Xóa PK (string)
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatSessions",
                table: "ChatSessions");

            // Bước 5: Xóa cột ChatSessionId (string) trong ChatSessions
            migrationBuilder.DropColumn(
                name: "ChatSessionId",
                table: "ChatSessions");

            // Bước 6: Thêm lại cột ChatSessionId (int) với IDENTITY
            migrationBuilder.AddColumn<int>(
                name: "ChatSessionId",
                table: "ChatSessions",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // Bước 7: Tạo PK (int)
            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatSessions",
                table: "ChatSessions",
                column: "ChatSessionId");

            // Bước 8: Thêm lại cột ChatSessionId (int) trong ChatMessages
            migrationBuilder.AddColumn<int>(
                name: "ChatSessionId",
                table: "ChatMessages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Bước 9: Tạo index
            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ChatSessionId",
                table: "ChatMessages",
                column: "ChatSessionId");

            // Bước 10: Tạo FK
            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_ChatSessions_ChatSessionId",
                table: "ChatMessages",
                column: "ChatSessionId",
                principalTable: "ChatSessions",
                principalColumn: "ChatSessionId",
                onDelete: ReferentialAction.Cascade);

            // Bước 11: Khôi phục cột SessionToken
            migrationBuilder.AddColumn<string>(
                name: "SessionToken",
                table: "ChatSessions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // Bước 12: Tạo unique index trên SessionToken
            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_SessionToken",
                table: "ChatSessions",
                column: "SessionToken",
                unique: true);
        }
    }
}
