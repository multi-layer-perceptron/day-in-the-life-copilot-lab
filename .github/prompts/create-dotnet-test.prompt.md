---
description: "Generate ContosoUniversity xUnit tests."
agent: agent
tools: ["read", "edit", "execute", "search"]
---

# Create .NET Test

Generate comprehensive xUnit tests for a ContosoUniversity class.

## Instructions

1. **Read the source file** specified by the user. If no file is specified, use the currently open file.
2. **Identify all public methods** that need testing
3. **Check existing test patterns** in `ContosoUniversity.Tests/`
4. **First, generate the test class setup** with Moq mocks in the constructor
5. **Then, add happy path tests** for expected successful behavior
6. **Then, add null or missing input tests** where parameters can be absent
7. **Then, add not found tests** for missing entities
8. **Then, add validation failure tests** for invalid model state or invalid input
9. **Finally, add error handling tests** for expected exception or failure paths

## Naming Convention

Use `MethodName_Condition_ExpectedResult`:
- `Index_WithStudents_ReturnsViewWithStudentList`
- `Details_NullId_ReturnsNotFound`
- `Create_ValidModel_RedirectsToIndex`

## After Generating

1. Build: `dotnet build ContosoUniversity.Tests/`
2. Run: `dotnet test ContosoUniversity.Tests/ --filter "{ClassName}"`
3. Report results
