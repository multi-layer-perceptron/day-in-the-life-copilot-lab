using ContosoUniversity.Core.Models;
using Xunit;

namespace ContosoUniversity.Tests.Models
{
    public class CourseTests
    {
        [Fact]
        public void MaxEnrollment_NewCourse_ReturnsDefaultValue()
        {
            // Arrange
            var course = new Course();

            // Act
            var result = course.MaxEnrollment;

            // Assert
            Assert.Equal(30, result);
        }
    }
}
