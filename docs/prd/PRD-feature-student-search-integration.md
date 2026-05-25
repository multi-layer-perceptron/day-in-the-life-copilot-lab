# PRD: Student Search Integration

**Branch:** `feature/student-search-integration`  
**Date:** 2026-05-25  
**Status:** Draft  
**Related Issues:** #N/A

---

## 1. Feature Overview

This feature enhances the student search capability within ContosoUniversity by integrating robust, multi-field search functionality into the Students module. Users with appropriate roles (Admin, Teacher) can search students by last name and/or first name, with results paginated and sortable by name or enrollment date. The integration ensures search state is preserved across pagination and sort operations.

---

## 2. User Stories

1. **As a Teacher**, I want to search for students by last name or first name so that I can quickly locate a specific student's record without scrolling through the full list.

2. **As an Admin**, I want search filters to persist when I navigate between pages of results so that I can review all matching students without re-entering my search terms.

3. **As an Admin**, I want to sort search results by name or enrollment date so that I can efficiently identify the most recently enrolled students matching my criteria.

---

## 3. Acceptance Criteria

### User Story 1 — Multi-field search
- [ ] The Students Index page (`/Students`) displays a search form with separate **Last Name** and **First Name** input fields.
- [ ] Submitting the form filters results using a case-insensitive `Contains` match on `Student.LastName` and/or `Student.FirstMidName`.
- [ ] Searching with both fields applies both filters simultaneously (AND logic).
- [ ] An empty search field does not filter on that field.
- [ ] A **Back to Full List** link clears all filters and returns to unfiltered results.

### User Story 2 — Filter persistence across pagination
- [ ] Clicking **Previous** or **Next** retains the active search filters in query string parameters (`currentLastNameFilter`, `currentFirstNameFilter`).
- [ ] Entering new search terms resets pagination to page 1 (`EffectivePageNumber = 1`).
- [ ] `StudentSearchQuery.IsNewSearch` correctly distinguishes a fresh search from a page navigation.

### User Story 3 — Sortable results
- [ ] Column headers **Full Name** and **Enrollment Date** are sortable links that toggle ascending/descending order.
- [ ] Sort state is preserved when navigating pages and does not clear active search filters.
- [ ] Default sort is ascending by `LastName`.

---

## 4. Technical Considerations

### Affected Projects
| Project | Changes |
|---|---|
| `ContosoUniversity.Web` | `StudentsController`, `StudentSearchQuery`, `Views/Students/Index.cshtml` |
| `ContosoUniversity.Core` | `IRepository<T>` — `GetQueryable()` used for deferred EF Core filtering |
| `ContosoUniversity.Infrastructure` | No changes expected; EF Core translates LINQ to SQL |
| `ContosoUniversity.Tests` | Unit tests for `StudentsController.Index` search/sort paths |

### Database Changes
- **No migrations required.** Search filters are applied in-memory via LINQ-to-EF on existing `Student` columns (`LastName`, `FirstMidName`). Ensure these columns are indexed in the database for performance at scale.

### Key Classes & Files
- **`StudentSearchQuery`** (`ContosoUniversity.Web/Models/StudentSearchQuery.cs`) — Immutable query object encapsulating sort, filter, and pagination state. `EffectiveLastNameFilter`, `EffectiveFirstNameFilter`, and `EffectivePageNumber` derive active values from new vs. continuing searches.
- **`StudentsController.Index`** (`ContosoUniversity.Web/Controllers/StudentsController.cs`) — Calls `IRepository<Student>.GetQueryable()`, applies `ApplyFilters()` and `ApplySorting()`, then creates a `PaginatedList<Student>`.
- **`IRepository<T>.GetQueryable()`** (`ContosoUniversity.Core/Interfaces/IRepository.cs`) — Returns `IQueryable<T>` to support deferred EF Core query building; do not use `GetAllAsync()` for filtered/sorted/paginated queries.
- **`Views/Students/Index.cshtml`** — Bootstrap 5 search form with `asp-route-*` Tag Helpers that propagate filter state through sort and pagination links.

### API Endpoints
| Method | Route | Purpose |
|---|---|---|
| `GET` | `/Students?lastNameSearch=&firstNameSearch=&sortOrder=&pageNumber=` | Filtered, sorted, paginated student list |

No new endpoints are introduced; all parameters are added to the existing `Index` action.

---

## 5. Testing Requirements

### Unit Tests (xUnit + Moq) — `ContosoUniversity.Tests/Controllers/StudentsControllerTests.cs`
- `Index_WithLastNameFilter_ReturnsOnlyMatchingStudents`
- `Index_WithFirstNameFilter_ReturnsOnlyMatchingStudents`
- `Index_WithBothFilters_AppliesCombinedAndLogic`
- `Index_WithNoFilters_ReturnsAllStudents`
- `Index_WithSortOrderNameDesc_ReturnsSortedDescending`
- `Index_WithSortOrderDate_ReturnsSortedByEnrollmentDateAscending`
- `Index_NewSearch_ResetsPageNumberToOne`

### Integration Tests (WebApplicationFactory) — `ContosoUniversity.Tests/Integration/`
- Verify the `/Students` endpoint returns HTTP 200 for authenticated Admin/Teacher users.
- Verify filter query strings are correctly reflected in the rendered HTML (e.g., input `value` attributes).
- Verify pagination links include the active filter parameters.

### E2E Tests (Playwright) — `ContosoUniversity.PlaywrightTests/`
- Search for a student by last name and verify the result table contains only matching rows.
- Search by first name and verify results.
- Navigate to page 2 of search results and verify the filter is preserved.
- Click **Back to Full List** and verify all students appear.

---

## 6. Out of Scope

- **Full-text / fuzzy search** — only `Contains` substring matching is in scope.
- **Enrollment course filtering** — searching by enrolled course, grade, or department is not included.
- **Export / download** — exporting filtered results to CSV or PDF is not in scope.
- **Real-time / AJAX search** — search is form-submitted (GET); live filtering without page reload is not included.
- **Role-based search restrictions** — all users with `Admin` or `Teacher` role see the same search fields.

---

## 7. Dependencies

- **`IRepository<T>.GetQueryable()`** must be implemented and return a live `IQueryable<T>` backed by EF Core (already in place).
- **`PaginatedList<T>`** (`ContosoUniversity.Web/Models/PaginatedList.cs`) must support async creation from `IQueryable<T>` (already in place).
- **Bootstrap 5** must be available in `wwwroot` for form and table styling (already in place).
- **ASP.NET Core Authorization** — `[Authorize(Roles = "Admin,Teacher")]` must be configured on the `Index` action (already applied).
