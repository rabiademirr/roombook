# Testing

## The contract
- Every acceptance criterion maps to at least one test (criterion ↔ test map lives in the plan).
- Tests assert **behavior**, not implementation details or mere status codes.
- The whole suite runs inside `scripts/check` — one command, everywhere.

## Frameworks & layout

- xUnit across all test projects.
- `RoomBook.Domain.Tests` and `RoomBook.Application.Tests` unit-test business rules directly
  against the Domain/Application types — no HTTP involved.
- `RoomBook.Api.Tests` uses `WebApplicationFactory<Program>` for end-to-end HTTP tests.
- Tests live under `tests/<ProjectName>.Tests/`, mirroring `src/<ProjectName>/`.

## What must be tested

- Every business rule (BR-1..6), including boundaries: exactly 09:00 start, exactly 18:00 end,
  exactly 15-minute duration, exactly 4-hour duration, exact back-to-back touching bookings.
- Free-slot suggestion correctness for a conflicting request on the same room.
- The forbidden-dependency architecture rule (Domain has no outward references) — an
  architecture test, not just review.

## Protected-tests rule
Weakening asserts, deleting, or skipping tests to reach green is forbidden. A red test triggers
`prompts/recovery/red-test.md` (R-02) — first decide what is wrong: code, test, or spec.

## Determinism
Flaky tests are fixed, not retried or skipped — see R-03. Evidence of a fix: 5 consecutive green runs.
</content>
