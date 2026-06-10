---
paths:
  - "src/features/admin/**"
---

# Admin Dashboard

The admin console at `/admin` (`AdminDashboardPage`). **Statistics and lists are static placeholders** for now — they get wired to admin query hooks (analytics, matches, business-rule config, audit log) when those slices land. The route is currently **ungated**; wrap it in `<ProtectedRoute role="Admin">` once admin role assignment exists.

## Layout
- Full-height `.wrap` = left `.sidebar` + `.main`.
- **Sidebar:** brand "WC26 Admin"; nav (Dashboard active, then Matches, Groups, Users, Business rules, Analytics, Notifications, Audit log, Export — each a `<button>` with chevron); footer admin-user card.
- **Topbar:** search box, "Live" pill (red dot), settings + bell icons, "AD" avatar.
- **Content:** page header + "Add match result" action; a 4-card stat grid; "Live & Upcoming Matches" (3 cards); a two-column bottom grid (Active business rules + Recent activity).

## Sections (static data lives in arrays at the top of the page)
- **Stat cards:** Registered users (14,382 +312), Total predictions (98,540 +4,210), Avg accuracy (34% −2%), Active groups (1,847 +58). Color variants `sc--green|blue|amber|purple`; delta chips `.up` (green) / `.warn` (amber).
- **Matches:** emoji flags + score/time; status `set` (Result set ✓), `live` (Live + dot), `soon` (Upcoming ⏱).
- **Business rules:** the configurable point values (Exact score 3, Correct winner/draw 1). There are no bonus or multiplier toggles.
- **Recent activity:** feed with colored icon chips (`act--green|blue|amber|purple|red`).

## Conventions
- **Styling:** `features/admin/pages/admin-dashboard.css`, **every selector scoped under `.wrap`** (see `styling.md`). Lime accent `#9BFF4A`; dark surfaces `#111`/`#161616`/`#1a1a1a`. **No inline styles** — per-card/per-activity colors are variant classes.
- **Icons:** inline SVG via `AdminIcon` (`features/admin/components/AdminIcon.tsx`); **no icon-font dependency**. Flags are emoji.
- **a11y:** nav items are buttons (`aria-current`), toggles `role="switch" aria-checked`, search `aria-label`, decorative SVG `aria-hidden`; responsive (stat grid → 2-up ≤1024px, sidebar stacks ≤720px).
- **Wiring later:** stats → analytics query; matches → calendar query; rules → config query + toggle mutations (audited); activity → audit-log query. Keep the page presentation-only; data in `features/admin/api` hooks (see `server-state-tanstack-query.md`, `components-presentation-only.md`).
