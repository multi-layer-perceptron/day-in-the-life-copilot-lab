using ContosoUniversity.Core.Models;
using ContosoUniversity.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ContosoUniversity.Tests.Infrastructure
{
    public class DbInitializerTests
    {
        [Fact]
        public async Task InitializeAsync_LegacyCourseTable_AddsMaxEnrollmentColumn()
        {
            // Arrange
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            await CreateLegacySchemaAsync(connection);

            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseSqlite(connection)
                .Options;

            await using var context = new SchoolContext(options);

            // Act
            await DbInitializer.InitializeAsync(context, NullLogger.Instance);

            // Assert
            var columnExists = await MaxEnrollmentColumnExistsAsync(connection);
            Assert.True(columnExists);

            var maxEnrollment = await GetMaxEnrollmentAsync(connection);
            Assert.Equal(Course.DefaultMaxEnrollment, maxEnrollment);
        }

        private static async Task CreateLegacySchemaAsync(SqliteConnection connection)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE "Person" (
                    "ID" INTEGER NOT NULL CONSTRAINT "PK_Person" PRIMARY KEY AUTOINCREMENT,
                    "LastName" TEXT NOT NULL,
                    "FirstName" TEXT NOT NULL,
                    "Discriminator" TEXT NOT NULL,
                    "EnrollmentDate" TEXT NULL,
                    "HireDate" TEXT NULL
                );

                CREATE TABLE "Course" (
                    "CourseID" INTEGER NOT NULL CONSTRAINT "PK_Course" PRIMARY KEY,
                    "Title" TEXT NOT NULL,
                    "Credits" INTEGER NOT NULL,
                    "DepartmentID" INTEGER NOT NULL,
                    "TeachingMaterialImagePath" TEXT NULL
                );

                INSERT INTO "Person" ("LastName", "FirstName", "Discriminator", "EnrollmentDate")
                VALUES ('Legacy', 'Student', 'Student', '2024-09-01');

                INSERT INTO "Course" ("CourseID", "Title", "Credits", "DepartmentID")
                VALUES (1000, 'Legacy Course', 3, 1);
                """;
            await command.ExecuteNonQueryAsync();
        }

        private static async Task<int> GetMaxEnrollmentAsync(SqliteConnection connection)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"""
                SELECT "{nameof(Course.MaxEnrollment)}"
                FROM "Course"
                WHERE "CourseID" = 1000;
                """;

            var result = await command.ExecuteScalarAsync();

            return (int)Assert.IsType<long>(result);
        }

        private static async Task<bool> MaxEnrollmentColumnExistsAsync(SqliteConnection connection)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"""
                SELECT COUNT(*)
                FROM pragma_table_info('Course')
                WHERE name = '{nameof(Course.MaxEnrollment)}';
                """;

            var result = await command.ExecuteScalarAsync();

            return Assert.IsType<long>(result) == 1;
        }
    }
}
