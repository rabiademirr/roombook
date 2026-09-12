# Conventions

## Language & framework versions

- C#, .NET 8.
- ASP.NET Core, Minimal API style (no `[ApiController]` classes).
- Nullable reference types enabled solution-wide.

## Naming

- PascalCase for types and public members; one public type per file, filename = type name.
- camelCase for locals and parameters.
- Projects: `RoomBook.Domain`, `RoomBook.Application`, `RoomBook.Infrastructure`, `RoomBook.Api`,
  mirrored by `RoomBook.Domain.Tests`, `RoomBook.Application.Tests`, `RoomBook.Api.Tests`.
- Test naming: class `<TypeUnderTest>Tests`; method `MethodName_Scenario_ExpectedResult`.

## Error handling

- Expected business-rule violations (BR-1..6, conflicts) are returned as a typed `Result<T>`
  outcome (e.g. Ok / Conflict / Invalid) from Application-layer services — never thrown as
  exceptions.
- Exceptions are reserved for true bugs/unexpected failures, not for rule violations.
- The Api layer maps outcomes to HTTP status codes (Ok→200/201, Conflict→409, Invalid→400) and
  never leaks exception details or stack traces in a response body.

## Data rules

- All timestamps are UTC, ISO-8601, represented as `DateTimeOffset` — never local/unspecified
  `DateTime`.
- Room and Booking IDs are `Guid` (assumption — no ID scheme was specified; revisit if wrong).

## Enforced by tooling

- `dotnet build --nologo -warnaserror` — treats warnings (including nullability) as errors.
- `dotnet test --nologo` — full suite must be green.
- Wired into `scripts/check.conf` once the solution exists (see report — currently a no-op).
