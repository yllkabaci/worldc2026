Seed the database with realistic World Cup 2026 demo data. Write to `application/backend/src/WorldCup.Infrastructure/Persistence/SeedData.cs` (create it if missing) and wire it behind a development-only seeding step.

Include:
- Real 2026 World Cup teams, venues, and kickoff times (UTC). No "Team A" / "Player 1" placeholders.
- ~20 user accounts with varied prediction histories and point totals.
- A few private groups (e.g. "Office League", "Family Cup").
- Official results for the first ~10 group-stage matches already confirmed and settled.
- Point totals computed per `SPEC.md` base scoring (exact 3 / winner 1 / draw 1; decimal; no bonuses/multipliers — those are tier 2).

Match the EF Core model and the `IApplicationDbContext` boundary; do not bypass EF. Keep it deterministic.
