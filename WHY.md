# WHY.md — Why We Exploit Claude Code More Than Other Teams

> Strategic reasoning for team alignment and judges' Q&A. Share before the hackathon.

---

## The core insight
Most teams use Claude Code like a senior developer: ask, get code, review, merge. That's the baseline, not the advantage. The advantage is treating Claude Code as a **system** — coordinated agents, automated checks, and specification-driven execution that runs faster and more correctly than anyone typing code.

The rubric rewards exactly this:

| Criteria | Weight | What it measures |
|---|---|---|
| Mission Ambition | 25% | Beyond a normal 3-hour build? |
| Specification Quality | 25% | Outcomes defined clearly enough to delegate? |
| Verification Rigor | 20% | Correctness proven by behavior, not code inspection? |
| Outcome Quality | 20% | Works, polished, no AI slop? |
| System Discipline | 10% | Did you follow the black-box rules? |

Three of five criteria (75%) reward the **quality of the process**, not feature count. We build better with a tighter system, not more.

## Why our spec corpus is the real multiplier
Other teams spend the lights-out phase watching the AI and redirecting it. We spend it monitoring a system that already knows what "right" looks like — because we defined it before the build. Our PRD isn't a vague feature list; it's a layered corpus:
- `SPEC.md` index + per-feature agent-grade specs in `specs/`
- Every business rule (BR-002…BR-022) with edge cases and resolved decisions
- A locked architecture (`backend/frontend-architecture.md`) + 13 binding `.claude/rules`
- A platform spec (`00-platform.md`) the build scaffolds from first

An agent given this executes; it doesn't guess. Guessing is what produces slop.

## Why hooks change the game
Hooks run automatically, without anyone remembering. Other teams rely on humans to run tests, check for slop, re-read the spec — under time pressure, humans forget. Our hooks (tests on edit, slop audit on stop, spec guard on write, context injection on submit) catch errors immediately, before they compound. Other teams spend 2:15–2:45 discovering the product is broken; we spend it confirming it's correct and polishing the demo.

## Why parallel agents change the math
A single session is sequential. Five people each running ~3 parallel agents = ~15 workstreams at once. The build phase is 90 minutes; sequential execution of 15 tasks doesn't fit. Parallel execution completes a batch in the time of the slowest task, then we fire another batch. That's how 5 people ship what would normally take 10.

## Why custom slash commands enforce consistency
Five sessions prompting differently produce inconsistent output. Shared commands are shared vocabulary: everyone's `/check-br` audits the same rules; `/verify-scenario SC-XX` returns a structured pass/fail against the exact scenario we defined; `/slop-check` applies the same `.claude/rules`. Consistency is what makes five people's work feel like one app.

## Why our verification is proof, not opinion
The rules require holdout scenarios written before the build and never shown to the agents. Ours are specific and live **outside the repo** so agents can't overfit:
- SC-11: edit blocked after the deadline even if the match is delayed (BR-007)
- SC-21: a rule change doesn't retroactively rescore past matches
- order-independence and points-conservation invariants

Passing these is evidence the engine implements the rules — not "it seemed to work when I clicked around." We demonstrate verification rigor with proof; other teams have opinions.

## Why the World Cup topic is a strategic moat
We're the only team building a live product for a tournament **happening right now (June 2026)**:
1. **Real data** — real teams, venues, kickoff times; judges see France vs Argentina, not Team A vs Team B.
2. **Live verification** — a judge can predict a real match this week and watch the points calculate.
3. **Immediate relevance** — everyone understands a prediction app; we don't spend demo time explaining the concept.

## Summary

| What other teams do | What we do |
|---|---|
| Write prompts, review code, redirect | Write the spec once; agents execute |
| Run tests when they remember | Tests run automatically on every edit |
| Check quality at the end | Hooks catch issues immediately |
| One task at a time | ~15 parallel workstreams |
| Vague verification ("it works") | Specific holdout scenarios with pass/fail, kept external |
| Generic demo, fake data | Live demo with real 2026 World Cup matches |
| Patch code manually when it breaks | Refine the spec, rerun the agent |

We're playing the game the hackathon was designed to reward.
