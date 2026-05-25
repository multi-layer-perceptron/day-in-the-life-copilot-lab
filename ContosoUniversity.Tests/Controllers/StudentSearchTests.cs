using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ContosoUniversity.Core.Interfaces;
using ContosoUniversity.Core.Models;
using ContosoUniversity.Web.Controllers;
using ContosoUniversity.Web.Models;

namespace ContosoUniversity.Tests.Controllers
{
    public class StudentSearchTests
    {
        private readonly Mock<IRepository<Student>> _mockStudentRepository;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ILogger<StudentsController>> _mockLogger;
        private readonly StudentsController _controller;

        public StudentSearchTests()
        {
            _mockStudentRepository = new Mock<IRepository<Student>>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockLogger = new Mock<ILogger<StudentsController>>();

            _controller = new StudentsController(
                _mockStudentRepository.Object,
                _mockNotificationService.Object,
                _mockLogger.Object);
        }

        #region Last Name Filtering

        [Fact]
        public async Task Index_LastNameFilter_ReturnsOnlyMatchingStudents()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act
            var result = await _controller.Index(null, null, null, "Smith", null, null);

            // Assert
            var model = GetModel(result);
            Assert.Equal(2, model.Count);
            Assert.All(model, s => Assert.Contains("Smith", s.LastName));
        }

        [Fact]
        public async Task Index_LastNameFilter_PartialMatch_ReturnsSubstringMatches()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act
            var result = await _controller.Index(null, null, null, "Alex", null, null);

            // Assert
            var model = GetModel(result);
            var student = Assert.Single(model);
            Assert.Equal("Alexandria", student.LastName);
        }

        [Fact]
        public async Task Index_LastNameFilter_NoMatch_ReturnsEmpty()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act
            var result = await _controller.Index(null, null, null, "Zzzzz", null, null);

            // Assert
            var model = GetModel(result);
            Assert.Empty(model);
        }

        [Fact]
        public async Task Index_LastNameFilter_EmptyString_ReturnsAllStudents()
        {
            // Arrange
            var students = CreateTestStudents();
            SetupRepository(students);

            // Act
            var result = await _controller.Index(null, null, null, "", null, null);

            // Assert
            var model = GetModel(result);
            Assert.Equal(students.Count, model.Count);
        }

        [Fact]
        public async Task Index_LastNameFilter_SetsCurrentLastNameFilterInViewData()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act
            var result = await _controller.Index(null, null, null, "Smith", null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Smith", viewResult.ViewData["CurrentLastNameFilter"]);
        }

        #endregion

        #region First Name Filtering

        [Fact]
        public async Task Index_FirstNameFilter_ReturnsOnlyMatchingStudents()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act
            var result = await _controller.Index(null, null, null, null, "Alex", null);

            // Assert
            var model = GetModel(result);
            var student = Assert.Single(model);
            Assert.Equal("Alex", student.FirstMidName);
            Assert.Equal("Parker", student.LastName);
        }

        [Fact]
        public async Task Index_FirstNameFilter_PartialMatch_ReturnsSubstringMatches()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act — "or" matches Jordan, Taylor, and Morgan
            var result = await _controller.Index(null, null, null, null, "or", null);

            // Assert
            var model = GetModel(result);
            Assert.Equal(3, model.Count);
            Assert.Contains(model, s => s.FirstMidName == "Jordan");
            Assert.Contains(model, s => s.FirstMidName == "Taylor");
            Assert.Contains(model, s => s.FirstMidName == "Morgan");
        }

        [Fact]
        public async Task Index_FirstNameFilter_NoMatch_ReturnsEmpty()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act
            var result = await _controller.Index(null, null, null, null, "Xxxxxx", null);

            // Assert
            var model = GetModel(result);
            Assert.Empty(model);
        }

        [Fact]
        public async Task Index_FirstNameFilter_SetsCurrentFirstNameFilterInViewData()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act
            var result = await _controller.Index(null, null, null, null, "Jordan", null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Jordan", viewResult.ViewData["CurrentFirstNameFilter"]);
        }

        #endregion

        #region Combined Filters

        [Fact]
        public async Task Index_CombinedFilters_ReturnsStudentsMatchingBoth()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act
            var result = await _controller.Index(null, null, null, "Smith", "Jordan", null);

            // Assert
            var model = GetModel(result);
            var student = Assert.Single(model);
            Assert.Equal("Jordan", student.FirstMidName);
            Assert.Equal("Smith", student.LastName);
        }

        [Fact]
        public async Task Index_CombinedFilters_NoOverlap_ReturnsEmpty()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act — "Alexandria" last name doesn't have "Jordan" first name
            var result = await _controller.Index(null, null, null, "Alexandria", "Jordan", null);

            // Assert
            var model = GetModel(result);
            Assert.Empty(model);
        }

        [Fact]
        public async Task Index_CombinedFilters_BothSetInViewData()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act
            var result = await _controller.Index(null, null, null, "Smith", "Zoe", null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Smith", viewResult.ViewData["CurrentLastNameFilter"]);
            Assert.Equal("Zoe", viewResult.ViewData["CurrentFirstNameFilter"]);
        }

        #endregion

        #region Sorting

        [Fact]
        public async Task Index_DefaultSort_OrdersByLastNameAscending()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act
            var result = await _controller.Index(null, null, null, null, null, null);

            // Assert
            var model = GetModel(result);
            var lastNames = model.Select(s => s.LastName).ToList();
            Assert.Equal(lastNames.OrderBy(n => n), lastNames);
        }

        [Fact]
        public async Task Index_NameDescSort_OrdersByLastNameDescending()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act
            var result = await _controller.Index("name_desc", null, null, null, null, null);

            // Assert
            var model = GetModel(result);
            var lastNames = model.Select(s => s.LastName).ToList();
            Assert.Equal(lastNames.OrderByDescending(n => n), lastNames);
        }

        [Fact]
        public async Task Index_DateSort_OrdersByEnrollmentDateAscending()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act
            var result = await _controller.Index("Date", null, null, null, null, null);

            // Assert
            var model = GetModel(result);
            var dates = model.Select(s => s.EnrollmentDate).ToList();
            Assert.Equal(dates.OrderBy(d => d), dates);
        }

        [Fact]
        public async Task Index_DateDescSort_OrdersByEnrollmentDateDescending()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act
            var result = await _controller.Index("date_desc", null, null, null, null, null);

            // Assert
            var model = GetModel(result);
            var dates = model.Select(s => s.EnrollmentDate).ToList();
            Assert.Equal(dates.OrderByDescending(d => d), dates);
        }

        [Fact]
        public async Task Index_SortWithFilter_AppliesBothCorrectly()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act — filter to Smiths, sort by date descending
            var result = await _controller.Index("date_desc", null, null, "Smith", null, null);

            // Assert
            var model = GetModel(result);
            Assert.Equal(2, model.Count);
            Assert.All(model, s => Assert.Contains("Smith", s.LastName));
            // Zoe (2023) should be first, Jordan (2020) second
            Assert.Equal("Zoe", model[0].FirstMidName);
            Assert.Equal("Jordan", model[1].FirstMidName);
        }

        [Fact]
        public async Task Index_SetsNameSortParmToNameDesc_WhenCurrentSortIsDefault()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act
            var result = await _controller.Index(null, null, null, null, null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("name_desc", viewResult.ViewData["NameSortParm"]);
        }

        [Fact]
        public async Task Index_SetsNameSortParmToEmpty_WhenCurrentSortIsNameDesc()
        {
            // Arrange
            SetupRepository(CreateTestStudents());

            // Act
            var result = await _controller.Index("name_desc", null, null, null, null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("", viewResult.ViewData["NameSortParm"]);
        }

        #endregion

        #region Pagination Reset

        [Fact]
        public async Task Index_NewLastNameSearch_ResetsPageToOne()
        {
            // Arrange
            SetupRepository(CreateManyStudents(25));

            // Act — providing lastNameSearch (non-null) should reset page
            var result = await _controller.Index(null, null, null, "Student", null, 3);

            // Assert
            var model = GetModel(result);
            Assert.Equal(1, model.PageIndex);
        }

        [Fact]
        public async Task Index_NewFirstNameSearch_ResetsPageToOne()
        {
            // Arrange
            SetupRepository(CreateManyStudents(25));

            // Act — providing firstNameSearch (non-null) should reset page
            var result = await _controller.Index(null, null, null, null, "First", 3);

            // Assert
            var model = GetModel(result);
            Assert.Equal(1, model.PageIndex);
        }

        [Fact]
        public async Task Index_NoNewSearch_PreservesCurrentPage()
        {
            // Arrange
            SetupRepository(CreateManyStudents(25));

            // Act — both search params null, use currentFilters to preserve existing filter
            var result = await _controller.Index(null, null, null, null, null, 2);

            // Assert
            var model = GetModel(result);
            Assert.Equal(2, model.PageIndex);
        }

        [Fact]
        public async Task Index_NoNewSearch_UsesCurrentLastNameFilter()
        {
            // Arrange
            SetupRepository(CreateManyStudents(25));

            // Act — null search params with currentLastNameFilter set
            var result = await _controller.Index(null, "Student", null, null, null, 2);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Student", viewResult.ViewData["CurrentLastNameFilter"]);
        }

        [Fact]
        public async Task Index_NoNewSearch_UsesCurrentFirstNameFilter()
        {
            // Arrange
            SetupRepository(CreateManyStudents(25));

            // Act — null search params with currentFirstNameFilter set
            var result = await _controller.Index(null, null, "First", null, null, 2);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("First", viewResult.ViewData["CurrentFirstNameFilter"]);
        }

        [Fact]
        public async Task Index_Pagination_ReturnsCorrectPageSize()
        {
            // Arrange — 15 students, page size is 10
            SetupRepository(CreateManyStudents(15));

            // Act
            var result = await _controller.Index(null, null, null, null, null, 1);

            // Assert
            var model = GetModel(result);
            Assert.Equal(10, model.Count);
            Assert.True(model.HasNextPage);
            Assert.False(model.HasPreviousPage);
        }

        [Fact]
        public async Task Index_Pagination_SecondPageHasRemainder()
        {
            // Arrange — 15 students, page size is 10
            SetupRepository(CreateManyStudents(15));

            // Act
            var result = await _controller.Index(null, null, null, null, null, 2);

            // Assert
            var model = GetModel(result);
            Assert.Equal(5, model.Count);
            Assert.False(model.HasNextPage);
            Assert.True(model.HasPreviousPage);
        }

        #endregion

        #region Helpers

        private void SetupRepository(List<Student> students)
        {
            _mockStudentRepository
                .Setup(repo => repo.GetQueryable())
                .Returns(students.AsQueryable());
        }

        private static PaginatedList<Student> GetModel(IActionResult result)
        {
            var viewResult = Assert.IsType<ViewResult>(result);
            return Assert.IsAssignableFrom<PaginatedList<Student>>(viewResult.Model);
        }

        private static List<Student> CreateTestStudents()
        {
            return new List<Student>
            {
                new Student { ID = 1, FirstMidName = "Jordan", LastName = "Smith", EnrollmentDate = DateTime.Parse("2020-09-01") },
                new Student { ID = 2, FirstMidName = "Alex", LastName = "Parker", EnrollmentDate = DateTime.Parse("2021-09-01") },
                new Student { ID = 3, FirstMidName = "Taylor", LastName = "Alexandria", EnrollmentDate = DateTime.Parse("2022-09-01") },
                new Student { ID = 4, FirstMidName = "Zoe", LastName = "Smith", EnrollmentDate = DateTime.Parse("2023-09-01") },
                new Student { ID = 5, FirstMidName = "Morgan", LastName = "Lee", EnrollmentDate = DateTime.Parse("2019-09-01") }
            };
        }

        private static List<Student> CreateManyStudents(int count)
        {
            return Enumerable.Range(1, count)
                .Select(i => new Student
                {
                    ID = i,
                    FirstMidName = $"First{i}",
                    LastName = $"Student{i:D2}",
                    EnrollmentDate = DateTime.Parse("2020-01-01").AddMonths(i)
                })
                .ToList();
        }

        #endregion
    }
}
