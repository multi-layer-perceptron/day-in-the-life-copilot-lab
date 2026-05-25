using ContosoUniversity.Core.Models;
using Xunit;

namespace ContosoUniversity.Tests.Models
{
    public class PersonFullNameTests
    {
        [Fact]
        public void FullName_ValidLastAndFirstName_ReturnsLastNameCommaFirstName()
        {
            // Arrange
            var student = new Student
            {
                LastName = "Smith",
                FirstMidName = "John"
            };

            // Act
            var result = student.FullName;

            // Assert
            Assert.Equal("Smith, John", result);
        }

        [Fact]
        public void FullName_StudentInstance_ReturnsCorrectFormat()
        {
            // Arrange
            var student = new Student
            {
                ID = 1,
                LastName = "Alexander",
                FirstMidName = "Carson",
                EnrollmentDate = DateTime.Parse("2020-09-01")
            };

            // Act
            var result = student.FullName;

            // Assert
            Assert.Equal("Alexander, Carson", result);
        }

        [Theory]
        [InlineData("O'Brien", "Miles", "O'Brien, Miles")]
        [InlineData("De La Cruz", "Ana Maria", "De La Cruz, Ana Maria")]
        [InlineData("Li", "W", "Li, W")]
        public void FullName_VariousNames_ReturnsExpectedFormat(
            string lastName, string firstMidName, string expected)
        {
            // Arrange
            var student = new Student
            {
                LastName = lastName,
                FirstMidName = firstMidName
            };

            // Act
            var result = student.FullName;

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
