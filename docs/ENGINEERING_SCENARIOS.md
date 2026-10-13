# Required engineering scenarios

The brief calls for **greenfield**, **brownfield**, and **ambiguous** work, each with task decomposition, execution and validation. These are demonstrations within one evolving service—not three unrelated apps.

## 1. Greenfield: URL creation + redirect
**Intent:** Give a consumer a short code that redirects to a target URL.

**Decomposition and dependencies**
1. Normalize requirements: input URL, unique code, redirect status, persistence.
2. Define request and response contracts.
3. Define ShortLink entity and unique index.
4. Implement collision-resistant code generation and URL validation.
5. Implement POST and GET controllers and error policy.
6. Add tests for success, invalid input, unknown code and collisions.

**Execution evidence:** `ShortLink.cs`, `ShortenerDbContext.cs`, service/controllers, test `CreateRedirectAnalyticsAndDisable_WorksEndToEnd` and `DuplicateCustomAlias_ReturnsConflict`.

**Validation:** 201 with short URL; 302 redirect; 400 for invalid URL; 404 missing code; 409 duplicate custom alias.

## 2. Brownfield: add click analytics + disable without breaking redirect
**Existing flow impact:** Redirect controller calls service, service queries SQLite. Analytics adds a counter and last-access time; disabling introduces `IsActive` check. No new public redirect contract is required.

**Decomposition**
1. Identify affected entity and data-flow.
2. Add counter, last-access time and active state.
3. Replace non-atomic counter update with an atomic SQL update.
4. Implement analytics read endpoint and idempotent disable endpoint.
5. Validate redirect remains 302 when active and becomes 410 once disabled.

**Validation:** integration test creates, redirects, observes click count, disables, and verifies 410. **Additional recommended test:** concurrent requests in a real database environment.

**Change-management note:** Implement as a separate Git commit or pull request and record actual before/after test output.

## 3. Ambiguous: "analytics and reliability features"
**Ambiguities:** What analytics? How long are links valid? Should codes be reusable? What counts as a failed visit? How are collisions handled? Can anyone disable a link?

**Explicit assumptions made for this prototype**
- Analytics = aggregate click count + last accessed UTC, no PII.
- No expiration unless specified; optional `expiresInDays` is 1–365.
- Disabled links and expired links return 410; codes are never reused.
- Failed/expired attempts are not counted; only successful resolution is counted.
- Duplicate custom aliases yield 409; generated codes retry up to 8 times.
- No user ownership/authentication in this prototype; production release would be blocked on this.

**Execution:** service business rules, error middleware, integration tests, documentation.

**Validation:** invalid expiration, deactivated links, collision checks and health endpoint. Formal load/security tests still pending.

## Engineer sign-off checklist
- [ ] Run all tests locally or in CI and save actual output
- [ ] Inspect request validation and security assumptions
- [ ] Validate invalid/edge inputs in Swagger
- [ ] Check code diff, perform review and record decisions
- [ ] Verify secrets/private assignment materials were not committed
- [ ] Record known risks and approving engineer
