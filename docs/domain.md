# Domain

> The shared language between business, humans, and agents. Same word = same meaning, everywhere.

## Ubiquitous language

| Term | Meaning | Notes / not to be confused with |
|---|---|---|
| Room | A bookable meeting room, identified by id and name | Not a building or office |
| Booking | A reservation of one Room for a contiguous UTC time range | Not a "hold" or pending request — always confirmed |
| Time slot | A start/end UTC timestamp pair | Must align to 15-minute increments (BR-6) |
| Conflict | Two bookings on the same Room whose time slots overlap | Touching boundaries are NOT a conflict (BR-3) |
| Free slot | A gap between existing bookings on a Room, within business hours | Suggested only on the same room the caller requested (v1 scope) |
| Business hours | 09:00–18:00 UTC, the only bookable window | Single office, single timezone — no per-room hours |

## Business rules

- BR-1: A booking's time slot must lie within business hours (09:00–18:00 UTC).
- BR-2: Bookings in the same room never overlap.
- BR-3: Back-to-back is allowed — one booking may start exactly when another ends.
- BR-4: Duration is 15 minutes to 4 hours.
- BR-5: All times are UTC, ISO-8601.
- BR-6: Start and end times must align to 15-minute increments (:00/:15/:30/:45).

## Key domain invariants

- No Room ever has two overlapping Bookings (BR-2, BR-3) — enforced before a Booking is accepted,
  never as a post-hoc cleanup.
- Every persisted Booking already satisfies BR-1, BR-4, BR-6 by construction — invalid states are
  never stored, even transiently.
