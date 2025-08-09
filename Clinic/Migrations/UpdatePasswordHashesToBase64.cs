using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePasswordHashesToBase64 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update Campus Clinic Staff (Role 2) - Password: admin123
            // SHA256 Base64: jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=
            migrationBuilder.Sql(
                "UPDATE [Clinic].[User] SET [PasswordHash] = 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=' WHERE [Role] = 2"
            );

            // Update Doctors (Role 1) - Password: doctor123
            // SHA256 Base64: BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=
            migrationBuilder.Sql(
                "UPDATE [Clinic].[User] SET [PasswordHash] = 'BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=' WHERE [Role] = 1"
            );

            // Update Students (Role 0) - Password: student123
            // SHA256 Base64: wA4qZoVJPzY+mCBEqpCLHIVF7HfrKiJJU7aWfvgWBDM=
            migrationBuilder.Sql(
                "UPDATE [Clinic].[User] SET [PasswordHash] = 'wA4qZoVJPzY+mCBEqpCLHIVF7HfrKiJJU7aWfvgWBDM=' WHERE [Role] = 0"
            );

            // Update UpdatedAt timestamp for all affected users
            migrationBuilder.Sql(
                "UPDATE [Clinic].[User] SET [UpdatedAt] = SYSUTCDATETIME()"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Optionally revert to previous hashes, but typically not needed
        }
    }
}