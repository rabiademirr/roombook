# Security

> Baseline rules agents must honor in every plan and review.

## Secrets
- Secrets never enter the repo, specs, prompts, or chat. `.env` is gitignored; provide `.env.example`.
- Agents never print secret values, even when debugging.

## Input & output
- Every request DTO is validated at the Api boundary: malformed JSON, missing fields, invalid time
  ranges, and BR violations all return 4xx with a generic message.
- No internal exception details or stack traces are ever returned in a response body.

## AuthN / AuthZ
- **None in v1** — this is a deliberate, documented scope decision (single office, no auth), not
  an oversight. All endpoints are open.
- This is a known gap that must be closed before any real/production deployment — track it as an
  open decision for whoever plans v2.

## Dependencies
- No new dependency is added without checking it's actively maintained and license-compatible.
- v1 forbids any database/ORM or authentication/identity package (see `docs/architecture.md`)
  until a future spec explicitly introduces them.

## Review lens
Security is a mandatory dimension of every independent review (see `prompts/review.md`), not a
separate afterthought phase.
