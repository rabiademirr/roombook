# Spec 0001 — Create booking with conflict detection and free-slot suggestions

- Status: Draft
- Mode: lite (from AGENTS.md at creation time)
- Plan: `specs/plans/0001-plan.md`

## Intent

RoomBook needs an operation for booking a meeting room: given a room and a requested time range,
the system either confirms the booking or rejects it when it conflicts with an existing booking
on that room. On rejection, the caller needs enough information to act — which existing
booking(s) block the request, and where the nearest actually-free slots are on that same room.
Success looks like: a caller requests a slot and gets a definitive, actionable answer (booked, or
here's what's blocking you and here's what *is* open) without polling or guessing. Deliberately
not being done here: room creation/management, authentication, alternative-room suggestions, or
recurring bookings.

## Requirements

- A caller can request a booking by specifying a room, a start time, and an end time.
- Rooms come from a small, fixed, pre-seeded set (room management is out of scope for this spec).
- A request succeeds only if it satisfies every business rule in `docs/domain.md` (BR-1..BR-6):
  within business hours, 15 minutes to 4 hours long, 15-minute-aligned, UTC/ISO-8601, and
  non-overlapping with every existing booking on that room (touching boundaries are not overlaps).
- A request that violates a business rule (hours, duration, alignment) is rejected as **invalid**
  — distinct from a rejection caused by a scheduling conflict.
- A request for a room outside the fixed seed set is rejected as **not found** — distinct from
  both invalid and conflict rejections.
- A request that overlaps one or more existing bookings on the same room is rejected as a
  **conflict**. The rejection identifies *every* existing booking that overlaps (there may be
  more than one — e.g. a long request spanning two shorter existing bookings), each with its time
  range and organizer.
- A conflict rejection also includes up to 3 free slots on the *same* room, matching the
  requested duration, within the *same business day* as the request, chosen by absolute distance
  from the requested start time (candidates before and after the request are both considered; on
  a tie, prefer the later slot). If fewer than 3 such slots exist — including none — the response
  reflects that (an empty list is a valid, non-error outcome, not a failure).

## Constraints & out of scope

- No authentication/authorization (v1 scope, `docs/security.md`) — every request is treated as
  trusted.
- No room creation, editing, or deletion — the seed set is fixed for this spec.
- No suggestions on rooms other than the one requested.
- No persistence beyond in-memory storage — data does not survive a restart.
- No recurring bookings, no attendee-count/capacity checks.
- Suggestions never cross into the next business day.

## Acceptance criteria

- [ ] AC-1 — A valid request (in business hours, 15min–4h, 15-min aligned, no conflict) on a
      seeded room succeeds.
- [ ] AC-2 — A request that fully overlaps an existing booking is rejected as a conflict, and the
      response lists that booking's time range and organizer.
- [ ] AC-3 — A request that overlaps only the start of an existing booking is rejected as a
      conflict.
- [ ] AC-4 — A request that overlaps only the end of an existing booking is rejected as a
      conflict.
- [ ] AC-5 — A request that fully contains a shorter existing booking is rejected as a conflict.
- [ ] AC-6 — A request spanning two separate existing bookings (with a gap between them) is
      rejected and **both** existing bookings are listed as conflicts.
- [ ] AC-7 — A request that starts exactly when an existing booking ends succeeds (back-to-back,
      BR-3) — not treated as a conflict.
- [ ] AC-8 — A request that ends exactly when an existing booking starts succeeds (back-to-back
      on the other side).
- [ ] AC-9 — A request starting before 09:00 UTC or ending after 18:00 UTC is rejected as
      **invalid** (BR-1), distinct from a conflict rejection.
- [ ] AC-10 — A request shorter than 15 minutes is rejected as invalid (BR-4).
- [ ] AC-11 — A request longer than 4 hours is rejected as invalid (BR-4).
- [ ] AC-12 — A request whose start or end isn't aligned to a 15-minute increment is rejected as
      invalid (BR-6).
- [ ] AC-13 — A request for a room not in the seed set is rejected as **not found**, distinct
      from invalid and conflict rejections, and includes no conflicts or suggestions.
- [ ] AC-14 — A conflict rejection includes up to 3 free slots on the same room, matching the
      requested duration, within the same business day, ordered by distance from the requested
      start time.
- [ ] AC-15 — When the room has no free slot of the requested duration left in the business day,
      the conflict rejection has an empty suggestions list (not an error).
- [ ] AC-16 — All times in requests and responses are UTC, ISO-8601 (BR-5).

## Self-critique (gaps found, resolved per lite mode — state assumption, proceed unless objected)

- **Gap:** No explicit tie-break rule when two candidate free slots are equally distant (one
  earlier, one later). **Resolution:** prefer the later slot on ties (added to Requirements above).
- **Gap:** "Overlap" wasn't explicit about the case where the *existing* booking is entirely
  inside the *requested* range (full containment), not just partial overlap. **Resolution:**
  added as AC-5, explicitly a conflict.
- **Gap:** Unclear whether a not-found room should still compute conflicts/suggestions.
  **Resolution:** no — not-found short-circuits before any conflict/suggestion logic (AC-13).
- **Gap:** Exposing an existing booking's organizer name to any caller (no-auth v1) is a
  deliberate scope choice already covered by `docs/security.md`'s documented v1 gap, not a new
  risk introduced here — noted, not treated as blocking.

## Review outcome

Independent review (read-only reviewer, diff `85f7296..b9e2155`) found 7 findings across the 6
review dimensions; performance and layering/forbidden-dependencies were clean. Triage:

- **Fixed (6, real):** missing organizer/title validation (commit `3d45ec8`); conflict test
  weakness — full-overlap scenario + time-range assertion (`d0bdfbe`); Api test isolation —
  per-test `WebApplicationFactory` instead of a shared singleton (`22278fd`); plan test-map
  correction for AC-6 (`33cf430`); missing `end == 18:00` boundary test (`11bc0a6`);
  one-type-per-file convention violations in 3 files (`3aee572`).
- **Rejected as noise (1):** a dev-environment-only stack-trace leak on malformed JSON. Verified
  clean under `ASPNETCORE_ENVIRONMENT=Production` (the config that matters for any real
  deployment); no acceptance criterion in this spec covers error-detail leakage, and the fix
  (explicit exception-handling middleware) is a general API hardening concern, not something this
  feature introduced. Logged as a future hardening idea, not a blocker for spec 0001.

## QA verification outcome

Independent QA pass (separate read-only agent, per invariant #3) mapped every AC to a named,
re-read, non-tautological passing test — all 16 criteria have real evidence, `./scripts/check`
green (36/36 at the time of the pass). It found one real coverage gap: `FreeSlotFinder`'s gap
computation (the cursor/sorted-bookings loop) was only exercised against a fully-empty or
fully-booked day, never against real partial bookings — the exact scenario AC-14 describes. Fixed
(commit `7b49b4f`): added a case with two non-adjacent existing bookings, asserting the exact
ranked slot list; passed on first run.

Lower-value gaps noted but accepted, not fixed (diminishing returns for a v1 prototype): 7 of 16
ACs (AC-3/4/5/8/9/11/12) have Domain-level evidence only, no Application/Api-level test for that
specific geometry — acceptable because the wiring connecting them (`CreateBookingService`'s single
overlap check, `Program.cs`'s single Invalid→400 mapping) is generic and already proven by the ACs
that do have integration-level tests (AC-2, AC-6, AC-7, AC-10). The plan's criterion↔test map also
has some stale test names from early refactors (cosmetic documentation drift, not re-verified
against the repo) — left as a known follow-up rather than reworked here.

## Definition of Done

- [x] Every acceptance criterion mapped to proof (test or reproducible observation)
- [x] `scripts/check` green
- [x] Independent review done; real findings fixed, noise rejected with written rationale
- [x] Docs / ADRs updated if behavior or architecture changed
- [ ] Spec moved to `specs/done/` (it becomes immutable there)

## Scorecard (fill at ship — honest numbers make the process improvable)
| Metric | Value |
|---|---|
| Spec revisions | |
| Fix rounds | |
| Review findings: real / noise | |
| Regressions introduced | |
| Bugs escaped to production | |
