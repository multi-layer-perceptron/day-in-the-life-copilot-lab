# AGENTS.md
<!-- Keep this file minimal: only include things agents can't discover on their own. -->

## Project Overview

This is the **Everything GitHub Copilot Hands-On Lab**. Use this file as a quick task-focused index for facts agents cannot easily discover: project shape, available agents, MCP servers, domain entities, and non-negotiable workflow boundaries.

## Technology Stack

| Category | Use When | Example |
| --- | --- | --- |
| Application runtime | Modifying ContosoUniversity web behavior | ASP.NET Core 8 MVC with Entity Framework Core |
| Architecture | Placing project files or data access code | Core, Infrastructure, Web, Tests, and PlaywrightTests |
| Copilot configuration | Updating agentic lab assets | Agents, skills, prompts, hooks, instructions, and MCP servers |
| CI/CD | Working on automation | GitHub Agentic Workflows for PRD generation and code review |

## Build & Test

- Build: `dotnet build ContosoUniversity.sln`
- Test: `dotnet test`

If an agent encounters an error or unexpected behavior, log the error and notify the user with a clear message.

## Agent Suite

If an agent encounters an error, log the error and notify the user with a clear message. Stop the affected workflow until the user confirms the next step.

### Azure Specialists

| Agent | Codename | Domain |
| --- | --- | --- |
| Infrastructure Architect | **Stratus** | Bicep IaC, Landing Zones, WAF |
| Agent Development | **Nexus** | Agent Framework SDK, MCP |
| Fabric Data Architect | **Prism** | OneLake, medallion patterns |
| Foundry Platform Engineer | **Forge** | Model catalog, Prompt Flow |
| SDET & Quality Engineer | **Sentinel** | Testing, chaos engineering |
| Suite Orchestrator | **Conductor** | Task decomposition, coordination |

### Development Agents

| Agent | Purpose |
| --- | --- |
| `dev` | General development with full tool access |
| `qa` | Testing specialist |
| `pm` | Product manager — requirements and acceptance criteria |
| `orchestrator` | Multi-agent workflow coordination |
| `code-reviewer` | Code review with high signal-to-noise ratio |
| `planner` | Feature planning and architecture |
| `architect` | System design and technical decisions |
| `tdd-guide` | Test-driven development enforcement |
| `security-reviewer` | Security vulnerability detection |

## MCP Servers

| Server | Type | Use For |
| --- | --- | --- |
| `context7` | stdio | Third-party library docs, SDKs, frameworks |
| `memory` | stdio | Knowledge graph for persisting entities across sessions |
| `sequential-thinking` | stdio | Structured chain-of-thought reasoning |
| `workiq` | stdio | Microsoft Work IQ for productivity |
| `microsoft-learn` | http | Azure services, Bicep, WAF, Microsoft products |

## ContosoUniversity Domain

The .NET project models a university system with these entities:

- **Student** — enrolled in courses, has enrollment date
- **Course** — has credits, belongs to department, has enrollments
- **Instructor** — teaches courses, has office assignment
- **Department** — manages courses, has administrator (instructor)
- **Enrollment** — links students to courses with optional grade

## Boundaries

- Keep generated documentation and configuration changes scoped to the current lab task.
- For Git operations, add files individually and do not use `git add .` or `git add -A`.
- For ContosoUniversity data access, use `IRepository<T>` instead of direct `SchoolContext` access from controllers.

## Architecture Decisions

### ADR-001: Repository Pattern for Data Access

**Status**: Accepted

**Context**: Controllers need database access but should not depend directly on Entity Framework's `SchoolContext`.

**Decision**: All data access goes through `IRepository<T>` defined in `ContosoUniversity.Core`. Implementations live in `ContosoUniversity.Infrastructure`.

**Consequences**:

- Controllers are testable with mock repositories
- Database technology can be swapped without changing controllers
- All queries go through a single abstraction layer

### Always Do

- Validate SKILL.md files have `name` and `description` frontmatter
- Test configurations in VS Code or Copilot CLI before marking complete
- Write session handoff documents at session end

### Never Do

- Hardcode secrets or API keys in any configuration file
- Use `git add .` or `git add -A`
- Create unnecessary documentation files
