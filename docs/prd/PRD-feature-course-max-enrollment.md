# PRD: Course Max Enrollment

**Branch:** `main`  
**Related Issues:** N/A  
**Status:** Implemented  
**Date:** 2026-05-26

---

## 1. Feature Overview

The Course model now includes a `MaxEnrollment` property to support enrollment caps. The property defaults to `30` for new courses and is validated with a minimum value of `1`.

---

## 2. Technical Details

### Model Changes

| Project | Change |
|---|---|
| `ContosoUniversity.Core` | Added `Course.DefaultMaxEnrollment = 30` and `Course.MaxEnrollment` with `[Range(1, int.MaxValue)]` validation |
| `ContosoUniversity.Infrastructure` | Updated `DbInitializer` to add the `MaxEnrollment` column when missing in brownfield databases |

### Schema Migration Approach

- `DbInitializer` checks whether the `Course.MaxEnrollment` column already exists before applying a schema update.
- SQL Server uses `ALTER TABLE ... ADD ... CONSTRAINT ... DEFAULT 30`.
- SQLite uses `ALTER TABLE ... ADD COLUMN ... DEFAULT 30`.
- This is a pragmatic brownfield approach for local and lab environments, but it is not ideal for production multi-instance deployments where coordinated migrations are preferred.

---

## 3. Testing

The feature is covered by targeted tests in `ContosoUniversity.Tests`:

- `CourseTests.MaxEnrollment_NewCourse_ReturnsDefaultValue` verifies the model default.
- `DbInitializerTests.InitializeAsync_LegacyCourseTable_AddsMaxEnrollmentColumn` verifies schema upgrade behavior and default backfill for legacy SQLite course rows.

Additional coverage would still be valuable for validation boundaries and migration idempotency.

---

## 4. Review Notes

- The current validation allows any value up to `int.MaxValue`; that may be more permissive than real-world course capacity limits.
- Schema updates inside `DbInitializer` are practical for this brownfield lab, but they should not be treated as the preferred production migration pattern.
- Test coverage could be expanded to include boundary validation scenarios and repeated initializer runs.
