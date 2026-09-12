# Architecture

## System overview

RoomBook is a meeting-room booking API for a single office (v1: in-memory storage, no auth,
single timezone/UTC). It exposes HTTP endpoints to create bookings and query availability,
rejecting conflicts (BR-2/BR-3) and suggesting free slots on the requested room when a request
can't be satisfied. Style: modular monolith, one .NET solution, layered strictly by dependency
direction so persistence can be swapped later without touching business rules.

## Modules / components and ownership

| Module | Single responsibility | Owns |
|---|---|---|
| RoomBook.Domain | Entities and business-rule invariants (BR-1..6) | `Room`, `Booking` |
| RoomBook.Application | Use-case orchestration: create booking, find free slots | Use-case services, `Result` outcome types, repository interfaces |
| RoomBook.Infrastructure | In-memory storage implementing Application's repository interfaces | In-memory Room/Booking store |
| RoomBook.Api | HTTP surface: minimal API endpoints, DTOs, validation, status-code mapping | HTTP request/response contracts |

## Communication rules

- Strict one direction: Api → Application → Domain.
- Infrastructure implements interfaces declared in Application; Application never references
  Infrastructure directly — only via DI at the Api composition root.
- Api never touches Domain types directly — always through an Application service.

## Forbidden dependencies (make them testable)

- Domain references nothing outside itself — no Application, Infrastructure, Api, or ASP.NET Core
  package.
- Application never references Infrastructure or Api, and carries no ASP.NET Core package
  reference.
- (Assert with an architecture test, e.g. NetArchTest, in RoomBook.Application.Tests.)

## Deliberately out of scope

- Persistence beyond in-memory (no database/ORM).
- Authentication/authorization.
- Multi-office or multi-timezone support.
- Recurring bookings.
- Room capacity / attendee-count checks.
- Alternative-room suggestions (free-slot suggestions cover the requested room only).
</content>
