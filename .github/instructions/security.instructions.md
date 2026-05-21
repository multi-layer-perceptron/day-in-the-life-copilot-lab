---
name: security
description: Security instructions for all files in the repository.
applyTo: "**/*"
---

# Security Instructions

These security rules apply to all files in this repository.

## Mandatory Checks Before Any Commit

- No hardcoded secrets (API keys, passwords, tokens) in source code.
- All user inputs validated with data annotations or FluentValidation.
- SQL injection prevention: use parameterized queries or EF Core. No string concatenation.
- XSS prevention: use Razor's built-in HTML encoding. Sanitize any raw HTML output.
- CSRF protection: use `[ValidateAntiForgeryToken]` on state-changing endpoints.
- Authentication and authorization verified on protected routes via `[Authorize]`.
- Rate limiting configured via `Microsoft.AspNetCore.RateLimiting` middleware.
- Error messages must not leak sensitive data, stack traces, or internal paths.

## Secret Management

```csharp
// NEVER: Hardcoded secrets
var apiKey = "sk-proj-xxxxx";

// ALWAYS: Use configuration and user secrets
var apiKey = builder.Configuration["ApiKey"];

if (string.IsNullOrEmpty(apiKey))
{
    throw new InvalidOperationException("ApiKey not configured");
}
```

## Input Validation

```csharp
using System.ComponentModel.DataAnnotations;

public class StudentCreateDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string LastName { get; init; } = default!;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string FirstMidName { get; init; } = default!;

    [DataType(DataType.Date)]
    public DateTime EnrollmentDate { get; init; }
}
```

## Authentication

- Use ASP.NET Core Identity or external providers for authentication.
- Store tokens in httpOnly cookies with Secure and SameSite=Strict flags.
- Use `[Authorize]` and policy-based authorization on controllers and actions.
- Never expose sensitive data in client-side state.

## Dependency Security

- Run `dotnet list package --vulnerable` regularly. Fix vulnerabilities before deploying.
- Use `dotnet restore --locked-mode` in CI/CD for reproducible builds.
- Keep NuGet packages up to date with `dotnet outdated`.
