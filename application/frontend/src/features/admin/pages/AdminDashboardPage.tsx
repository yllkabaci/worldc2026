import { AdminIcon } from "../components/AdminIcon";
import "./admin-dashboard.css";

// NOTE: statistics and lists are static placeholders for now. They will be wired to
// admin query hooks (analytics, matches, business rules, audit log) when those slices land.

const NAV = [
  { icon: "dashboard", label: "Dashboard", active: true, chevron: false },
  { icon: "ball", label: "Matches", active: false, chevron: true },
  { icon: "trophy", label: "Groups", active: false, chevron: true },
  { icon: "users", label: "Users", active: false, chevron: true },
  { icon: "sliders", label: "Business rules", active: false, chevron: true },
  { icon: "chart", label: "Analytics", active: false, chevron: true },
  { icon: "bell", label: "Notifications", active: false, chevron: true },
  { icon: "lock", label: "Audit log", active: false, chevron: true },
  { icon: "export", label: "Export", active: false, chevron: true },
];

const STATS = [
  { variant: "sc--green", icon: "users", value: "14,382", delta: "+312", dir: "up", tone: "up", period: "from yesterday", label: "Registered users" },
  { variant: "sc--blue", icon: "listCheck", value: "98,540", delta: "+4,210", dir: "up", tone: "up", period: "today", label: "Total predictions" },
  { variant: "sc--amber", icon: "target", value: "34%", delta: "-2%", dir: "down", tone: "warn", period: "vs yesterday", label: "Avg accuracy" },
  { variant: "sc--purple", icon: "usersGroup", value: "1,847", delta: "+58", dir: "up", tone: "up", period: "today", label: "Active groups" },
];

const MATCHES = [
  { group: "Group A", home: "🇫🇷", away: "🇲🇽", date: "Jun 12", center: "2 – 1", predicted: "245,000 predicted", status: { kind: "set", text: "Result set" } },
  { group: "Group B", home: "🇧🇷", away: "🇷🇸", date: "Jun 13", center: "1 – 0", predicted: "248,000 predicted", status: { kind: "live", text: "Live 62'" } },
  { group: "Group C", home: "🇺🇸", away: "🇦🇷", date: "Jun 14", center: "18:00 ET", predicted: "312,000 predicted", status: { kind: "soon", text: "Upcoming" } },
];

const RULES = [
  { icon: "trophy", name: "Exact score", pts: "3 pts", on: true },
  { icon: "award", name: "Correct winner / draw", pts: "1 pt", on: true },
];

const ACTIVITY = [
  { variant: "act--green", icon: "check", title: "Result confirmed — FRA 2-1 MEX", sub: "Points distributed to 245,000 users", time: "2m ago" },
  { variant: "act--blue", icon: "userPlus", title: "312 new users registered", sub: "Peak signup hour: 20:00–21:00", time: "6m ago" },
  { variant: "act--amber", icon: "sliders", title: "Business rule updated", sub: "Correct winner / draw: 1 pt (was 2)", time: "18m ago" },
  { variant: "act--purple", icon: "usersGroup", title: "58 new groups created", sub: 'Largest: "Balkans United" — 47 members', time: "34m ago" },
  { variant: "act--red", icon: "ban", title: "User account blocked", sub: "Reason: suspicious prediction pattern", time: "1h ago" },
  { variant: "act--green", icon: "lock", title: "Prediction window closed", sub: "BRA vs SRB — 60 min before kickoff", time: "1h ago" },
];

export function AdminDashboardPage() {
  return (
    <div className="wrap">
      <aside className="sidebar">
        <div className="sb-brand">
          <div className="sb-brand-icon"><AdminIcon name="ball" size={17} /></div>
          <span className="sb-brand-name">WC26 Admin</span>
        </div>
        {NAV.map((item) => (
          <button key={item.label} type="button" className={`nav-item${item.active ? " active" : ""}`} aria-current={item.active ? "page" : undefined}>
            <span className="left"><AdminIcon name={item.icon} size={16} /><span>{item.label}</span></span>
            {item.chevron && <AdminIcon name="chevron" size={13} className="chev" />}
          </button>
        ))}
        <div className="sb-footer">
          <div className="sb-avatar"><AdminIcon name="user" size={18} /></div>
          <div>
            <div className="sb-user-name">Admin User</div>
            <div className="sb-user-email">admin@wc2026.com</div>
          </div>
        </div>
      </aside>

      <div className="main">
        <header className="topbar">
          <div className="search-box">
            <AdminIcon name="search" size={15} />
            <input type="text" placeholder="Search matches, users, predictions..." aria-label="Search" />
          </div>
          <div className="tb-right">
            <span className="live-pill"><span className="live-dot" /> Live</span>
            <span className="tb-icon"><AdminIcon name="settings" size={18} /></span>
            <span className="tb-icon"><AdminIcon name="bell" size={18} /></span>
            <span className="tb-avatar">AD</span>
          </div>
        </header>

        <div className="content">
          <div className="page-header">
            <div>
              <h1>Dashboard Overview</h1>
              <p>Welcome back! Here's what's happening with your predictions platform.</p>
            </div>
            <button type="button" className="btn-action"><AdminIcon name="plus" size={15} /> Add match result</button>
          </div>

          <div className="stat-grid">
            {STATS.map((s) => (
              <div key={s.label} className={`stat-card ${s.variant}`}>
                <div className="stat-card-glow" />
                <div className="sc-top">
                  <div className="sc-icon"><AdminIcon name={s.icon} size={19} /></div>
                  <span className="sc-arrow"><AdminIcon name="arrowUpRight" size={15} /></span>
                </div>
                <div className="sc-val">{s.value}</div>
                <div>
                  <span className={`sc-delta ${s.tone}`}>
                    <AdminIcon name={s.dir === "up" ? "arrowUp" : "arrowDown"} size={11} /> {s.delta}
                  </span>
                  <span className="sc-label">{s.period}</span>
                </div>
                <div className="sc-sub">{s.label}</div>
              </div>
            ))}
          </div>

          <div className="section-header">
            <span className="section-title">Live &amp; Upcoming Matches</span>
            <button type="button" className="view-all">View all</button>
          </div>
          <div className="match-grid">
            {MATCHES.map((m) => (
              <div key={m.group} className="match-card">
                <div className="mc-top">
                  <span className="mc-comp">FIFA World Cup 2026™</span>
                  <span className="mc-badge">{m.group}</span>
                </div>
                <div className="mc-teams">
                  <span className="mc-flag" role="img" aria-label="home team">{m.home}</span>
                  <div className="mc-center">
                    <div className="mc-date">{m.date}</div>
                    <div className="mc-time">{m.center}</div>
                  </div>
                  <span className="mc-flag" role="img" aria-label="away team">{m.away}</span>
                </div>
                <div className="mc-bottom">
                  <span className="mc-watching"><AdminIcon name="users" size={13} /> {m.predicted}</span>
                  <span className={`mc-status ${m.status.kind}`}>
                    {m.status.kind === "set" && <AdminIcon name="check" size={13} />}
                    {m.status.kind === "live" && <span className="mc-live-dot" />}
                    {m.status.kind === "soon" && <AdminIcon name="clock" size={12} />}
                    {m.status.text}
                  </span>
                </div>
              </div>
            ))}
          </div>

          <div className="bottom-grid">
            <section className="panel">
              <div className="section-header">
                <span className="section-title">Active business rules</span>
                <button type="button" className="view-all">Configure</button>
              </div>
              {RULES.map((r) => (
                <div key={r.name} className="rule-row">
                  <span className="rule-left"><AdminIcon name={r.icon} size={14} /><span className="rule-name">{r.name}</span></span>
                  <span className="rule-right">
                    <span className="pts">{r.pts}</span>
                    <button type="button" className={`toggle${r.on ? "" : " off"}`} role="switch" aria-checked={r.on} aria-label={`${r.name} ${r.on ? "enabled" : "disabled"}`} />
                  </span>
                </div>
              ))}
            </section>

            <section className="panel">
              <div className="section-header">
                <span className="section-title">Recent activity</span>
                <button type="button" className="view-all">View all</button>
              </div>
              {ACTIVITY.map((a, i) => (
                <div key={i} className="act-row">
                  <div className={`act-icon ${a.variant}`}><AdminIcon name={a.icon} size={14} /></div>
                  <div className="act-body">
                    <div className="act-title">{a.title}</div>
                    <div className="act-sub">{a.sub}</div>
                  </div>
                  <span className="act-time"><AdminIcon name="clock" size={11} /> {a.time}</span>
                </div>
              ))}
            </section>
          </div>
        </div>
      </div>
    </div>
  );
}
