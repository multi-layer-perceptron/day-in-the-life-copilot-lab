## Continuing: Memory MCP Testing Pattern Discovery

Last commit: 722b947 "docs: update cleanup instructions for Git worktrees and branches"
Branch: main
Date: 2026-05-26 03:38:04 UTC

### Context

The immediate objective was to inspect the Memory MCP knowledge graph, summarize what it stores, and then derive ContosoUniversity testing patterns from those stored observations.

Important background decisions found in Memory MCP:
- ContosoUniversity chose the repository pattern over direct DbContext access for testability.
- Test names should follow `MethodName_Condition_ExpectedResult`.
- Production uses SQL Server, while integration tests use SQLite via WebApplicationFactory.
- Code review requires approval from the `@code-reviewer` agent before merge.
- ContosoUniversity uses Clean Architecture: Core has no dependencies, Infrastructure depends on Core, and Web depends on both.

Relevant constraints:
- Keep controllers behind `IRepository<T>` rather than direct `SchoolContext` access.
- Use focused unit tests for controller/service behavior and WebApplicationFactory for full MVC pipeline behavior.
- No related issue or PR was mentioned in the current work.

### State of Work

**✅ Done:**
- [x] Read the full Memory MCP knowledge graph.
- [x] Summarized stored entities, observations, and relations.
- [x] Derived testing guidance for ContosoUniversity from Memory MCP.
- [x] Checked git branch, recent commits, status, and diff stat.
- [x] Checked for prior handoff documentation.
- [x] Scanned current local agents and prompts for resume recommendations.

**🚧 In Progress:**
- [ ] None. The requested analysis is complete.

**⏸️ Not Started:**
- [ ] Apply the testing patterns to a concrete feature or bug fix.
- [ ] Add or update tests in ContosoUniversity.Tests if new implementation work begins.
- [ ] Run full build/test verification after any future code changes.

### Current Position

**Where We Stopped:**
- Work paused after creating this handoff document.
- No application source files were modified before this handoff.
- The active editor file was `.github/prompts/handoff.prompt.md`, which supplied the requested handoff structure.

**Why We Stopped:**
- Natural break: the Memory MCP summary and testing-pattern answer were completed.

### Key Discoveries

- Memory MCP currently stores three entities: `ContosoUniversity-Decisions`, `ContosoUniversity`, and `CleanArchitecture`.
- Memory MCP stores one relation: `ContosoUniversity` uses `CleanArchitecture`.
- The strongest testing rule stored in Memory MCP is the `MethodName_Condition_ExpectedResult` naming convention.
- Integration tests should use SQLite via WebApplicationFactory, not production SQL Server.
- Controller tests should mock repository boundaries rather than directly exercising `SchoolContext`.
- Existing local agent files are focused on ContosoUniversity .NET development, QA, planning, orchestration, and code review.
- No `handoff.md`, `HANDOFF.md`, or `docs/SESSION_HANDOFFS/` content exists in this repository; only `solutions/lab10-session-management/sample-handoff.md` was found as a sample.

### Files Modified

**Created:**
- `docs/handoffs/memory-mcp-testing-patterns-handoff-2026-05-26.md` - Captures the Memory MCP/testing-pattern session handoff.

**Modified:**
- None.

**Deleted:**
- None.

### Open Questions

1. Should future ContosoUniversity integration tests standardize exclusively on SQLite, or is an in-memory provider still acceptable for narrow Infrastructure tests?
2. Should the Memory MCP graph be updated with more detailed test practices from `.github/agents/dotnet-qa.agent.md`?
3. Should handoffs be stored consistently under `docs/handoffs/`, or should the repository add a formal `docs/SESSION_HANDOFFS/` directory to match the prompt text?

### Next Steps

1. **Immediate:** If continuing with test work, read Memory MCP again and open `.github/agents/dotnet-qa.agent.md` for the detailed xUnit/Moq/WebApplicationFactory patterns.
2. **Then:** Inspect existing tests in `ContosoUniversity.Tests/` before writing new tests.
3. **Finally:** Run focused tests first, then broaden to solution-level verification if shared behavior changed.

### Verification

**Tests that should pass:**
- Documentation-only handoff creation does not require application tests.
- If implementation or tests are added later, `dotnet test ContosoUniversity.Tests/` should pass.
- Before PR or merge, `dotnet test ContosoUniversity.sln` should pass.

**Commands to verify:**

```bash
git status --porcelain
git log --oneline -5
dotnet test ContosoUniversity.Tests/
dotnet test ContosoUniversity.sln
```

**Expected output:**
- `git status --porcelain` should show only the new handoff file unless additional work has started.
- Test commands should complete with all tests passing.

### Pitfalls to Avoid

- ❌ Do not access `SchoolContext` directly from controllers; use `IRepository<T>`.
- ❌ Do not name tests with vague behavior labels; use `MethodName_Condition_ExpectedResult`.
- ❌ Do not use production SQL Server for integration tests; use SQLite through WebApplicationFactory.
- ❌ Do not skip code review for merge-bound work; Memory MCP records `@code-reviewer` approval as required.

### Quick Reference

**Key files:**
- `.github/agents/dotnet-qa.agent.md`
- `.github/agents/dotnet-dev.agent.md`
- `.github/agents/code-reviewer.agent.md`
- `.github/prompts/create-dotnet-test.prompt.md`
- `.github/prompts/tdd.prompt.md`
- `.github/prompts/code-review.prompt.md`
- `.github/prompts/verify.prompt.md`

**Important functions/classes:**
- `IRepository<T>` - repository abstraction for controller and service tests.
- `WebApplicationFactory` - integration testing foundation for MVC pipeline tests.
- `SchoolContext` - EF Core context, used through Infrastructure/repository boundaries rather than controllers.

**Dependencies:**
- xUnit for unit/integration tests.
- Moq for mocking repositories and services.
- WebApplicationFactory for ASP.NET Core integration tests.
- SQLite for integration-test database behavior.

**Documentation links:**
- `solutions/lab10-session-management/sample-handoff.md` - existing handoff example.
- `.github/prompts/handoff.prompt.md` - source prompt for this handoff structure.

### Fresh Context Prompt

```text
## Resume Session

**Handoff:** docs/handoffs/memory-mcp-testing-patterns-handoff-2026-05-26.md
**Branch:** main
**Last Commit:** 722b947 "docs: update cleanup instructions for Git worktrees and branches"

### Quick Context
The previous session read the entire Memory MCP knowledge graph and summarized its stored ContosoUniversity decisions. It then translated those observations into testing guidance: use `MethodName_Condition_ExpectedResult`, mock repository boundaries, and use SQLite-backed WebApplicationFactory for integration tests.

### Immediate Next Step
1. If implementing tests, inspect existing patterns in ContosoUniversity.Tests and use `.github/agents/dotnet-qa.agent.md` as the testing guide.

### Read Memory
Use Memory MCP: memory-read_graph to get entity observations

### Recommended Agents
- dotnet-qa.agent.md - for xUnit, Moq, and WebApplicationFactory test coverage
- dotnet-dev.agent.md - for ContosoUniversity .NET implementation work
- code-reviewer.agent.md - for merge-bound review focused on bugs, security, architecture, and tests

### Recommended Prompts
- /create-dotnet-test - for generating ContosoUniversity xUnit tests
- /tdd - for test-first implementation
- /code-review - after implementation or test changes
```
