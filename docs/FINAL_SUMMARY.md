# Final engineering summary — draft pending engineer approval

## Goal and rationale
Deliver an end-to-end URL shortener and demonstrate human-directed AI-assisted implementation, test design, codebase reasoning, and quality controls. A modular ASP.NET Core architecture was chosen for ease of explanation and maintenance; SQLite makes the prototype portable.

## Completed as source artifacts
- Versioned URL create API and short-code redirect
- Custom aliases and cryptographically random Base62 code generation
- Expiration, deactivation, aggregate click analytics
- EF Core schema with unique code index
- Integration test source and CI workflow
- Three engineering scenarios and AI audit-trail template

## Verification disclosure
The file-generation environment does **not** provide `dotnet`. The code has not been compiled or test-executed here. You must run `dotnet restore`, `dotnet build`, `dotnet test`, inspect failures, and capture GitHub Actions evidence. Do not claim tests passed until verified.

## Risk and control matrix
| Risk | Mitigation in prototype | Remaining work |
|---|---|---|
| Alias collisions | Unique DB index; bounded retries; 409 | Load tests for burst creation |
| Click-count races | Atomic conditional `ExecuteUpdateAsync` | Concurrency/load test; SQLite write contention |
| Bad destination URL | HTTP(S) only; rejects local/private literals | More complete IP ranges, DNS/reputation/abuse handling |
| Unauthorized link changes | None | Add users, API keys/OIDC and ownership checks |
| Excessive traffic | None | Rate limits, alerts and abuse prevention |
| Schema evolution | `EnsureCreated` | EF Core migrations + deploy controls |
| Host header spoofing | None | AllowedHosts, proxy configuration, fixed public base URL |
| Click analytics scale | Simple synchronous aggregate | Event queue, partitioning or buffered analytics |
| Observability | Health endpoint, basic exception logging | Structured logs, traces, request metrics |
| Privacy | No visitor IP/PII stored | Document retention and privacy policy |

## Change control & sign-off
For an actual assessment, add meaningful commits/PRs, real CI logs, peer review if available, and explicit decisions (including rejected AI suggestions). Engineer approves correctness, quality and submission. Keep internal assignment materials out of any public repository.
