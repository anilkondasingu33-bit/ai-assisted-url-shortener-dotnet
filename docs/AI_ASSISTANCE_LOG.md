# AI-assisted engineering traceability

The assignment requires **intent, constraints, acceptance criteria, iterative prompting, review outcomes, and human approval**. The entries below distinguish work proposed by the assistant from actions that still require the engineer to perform. Do not present planned human sign-off as already done.

| ID | Task / prompt intent | Constraints and acceptance criteria | AI output | Human review status |
|---|---|---|---|---|
| A1 | Translate the URL shortener requirements into modular .NET design | REST, clear error responses, local runnable DB | Controllers, EF entity, service and architecture docs generated | **Pending your review** |
| A2 | Implement robust alias handling | DB uniqueness, conflict=409, random retry bounded | Unique index, exception mapping, Base62 code generator | **Pending local validation** |
| A3 | Improve redirect analytics correctness | Avoid lost updates; disabled links must not redirect | Database-side conditional click count update | **Pending concurrency/load review** |
| A4 | Generate regression tests | 201/302/400/404/409/410, health, analytics | xUnit integration tests generated | **Pending SDK test run** |
| A5 | Prepare submission artifacts | Explicit assumptions/trade-offs; 3 scenarios | README, architecture, scenarios, summary | **Pending engineer approval** |

## Reusable disciplined prompt
```
Context: ASP.NET Core 8 controllers, EF Core SQLite, URL shortener.
Intent: [exact engineering change].
Files impacted: [paths and current behavior].
Constraints: no controller-side business rules; atomic update where needed;
no secrets in prompts; backward-compatible API.
Acceptance criteria: [behavior and HTTP status], unit/integration test,
no regression, clear documentation.
Please propose a minimal diff, tests, and risks. Do not claim it was validated.
```

## What to record as you implement
- Date, prompt, AI tool/model, files changed.
- Accepted/edited/rejected suggestion and rationale.
- Local command, actual test/lint output, PR link.
- Engineer sign-off and unresolved risks.

## Example review decision (must verify yourself)
A naive increment `link.ClickCount++; SaveChangesAsync()` can lose updates on concurrent redirects. The proposed database-side `ExecuteUpdateAsync` increments atomically. Verify with integration or load tests before claiming concurrency guarantees.

## Secure AI use
Never paste production credentials, patient data, proprietary source code, or the internal assignment document into a public repo or unauthorized AI tool. Require human sign-off for releases and security-sensitive changes.
