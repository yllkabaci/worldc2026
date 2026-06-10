interface Props {
  name: string;
  size?: number;
  className?: string;
}

/** Minimal inline-SVG icon set for the admin shell (no icon-font dependency; inherits currentColor). */
export function AdminIcon({ name, size = 18, className }: Props) {
  const common = {
    width: size,
    height: size,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.8,
    strokeLinecap: "round" as const,
    strokeLinejoin: "round" as const,
    className,
    "aria-hidden": true,
  };

  switch (name) {
    case "ball":
      return (<svg {...common}><circle cx="12" cy="12" r="9" /><path d="M12 7.5l3.2 2.3-1.2 3.8h-4l-1.2-3.8z" fill="currentColor" stroke="none" /></svg>);
    case "dashboard":
      return (<svg {...common}><rect x="3" y="3" width="7" height="9" rx="1.5" /><rect x="14" y="3" width="7" height="5" rx="1.5" /><rect x="14" y="12" width="7" height="9" rx="1.5" /><rect x="3" y="16" width="7" height="5" rx="1.5" /></svg>);
    case "trophy":
      return (<svg {...common}><path d="M8 4h8v5a4 4 0 0 1-8 0z" /><path d="M8 5H5v2a3 3 0 0 0 3 3M16 5h3v2a3 3 0 0 1-3 3" /><path d="M10 14h4M9 20h6M12 14v3" /></svg>);
    case "users":
      return (<svg {...common}><circle cx="9" cy="8" r="3" /><path d="M3 20a6 6 0 0 1 12 0" /><path d="M16 5.5a3 3 0 0 1 0 5M21 20a6 6 0 0 0-4-5.6" /></svg>);
    case "usersGroup":
      return (<svg {...common}><circle cx="12" cy="8" r="3" /><path d="M6 20a6 6 0 0 1 12 0" /><circle cx="5" cy="9" r="2" /><circle cx="19" cy="9" r="2" /></svg>);
    case "sliders":
      return (<svg {...common}><path d="M4 6h10M18 6h2M4 12h4M12 12h8M4 18h12M20 18h0" /><circle cx="15" cy="6" r="2" /><circle cx="9" cy="12" r="2" /><circle cx="17" cy="18" r="2" /></svg>);
    case "chart":
      return (<svg {...common}><path d="M4 20V4" /><path d="M4 20h16" /><rect x="7" y="12" width="3" height="5" fill="currentColor" stroke="none" /><rect x="12" y="8" width="3" height="9" fill="currentColor" stroke="none" /><rect x="17" y="14" width="3" height="3" fill="currentColor" stroke="none" /></svg>);
    case "bell":
      return (<svg {...common}><path d="M6 9a6 6 0 0 1 12 0c0 5 2 6 2 6H4s2-1 2-6" /><path d="M10 20a2 2 0 0 0 4 0" /></svg>);
    case "lock":
      return (<svg {...common}><rect x="5" y="11" width="14" height="9" rx="2" /><path d="M8 11V8a4 4 0 0 1 8 0v3" /></svg>);
    case "export":
      return (<svg {...common}><path d="M14 3h7v7M21 3l-9 9" /><path d="M20 14v5a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h5" /></svg>);
    case "chevron":
      return (<svg {...common}><path d="M9 6l6 6-6 6" /></svg>);
    case "user":
      return (<svg {...common}><circle cx="12" cy="8" r="4" /><path d="M4 21a8 8 0 0 1 16 0" /></svg>);
    case "userPlus":
      return (<svg {...common}><circle cx="9" cy="8" r="4" /><path d="M3 21a6 6 0 0 1 12 0" /><path d="M19 8v6M16 11h6" /></svg>);
    case "search":
      return (<svg {...common}><circle cx="11" cy="11" r="7" /><path d="M21 21l-4.3-4.3" /></svg>);
    case "settings":
      return (<svg {...common}><circle cx="12" cy="12" r="3" /><path d="M12 2v3M12 19v3M2 12h3M19 12h3M5 5l2 2M17 17l2 2M19 5l-2 2M7 17l-2 2" /></svg>);
    case "plus":
      return (<svg {...common}><path d="M12 5v14M5 12h14" /></svg>);
    case "listCheck":
      return (<svg {...common}><path d="M9 6h11M9 12h11M9 18h11" /><path d="M3.5 6l1.2 1.2L7 5M3.5 12l1.2 1.2L7 11M3.5 18l1.2 1.2L7 17" /></svg>);
    case "target":
      return (<svg {...common}><circle cx="12" cy="12" r="8" /><circle cx="12" cy="12" r="4" /><circle cx="12" cy="12" r="1" fill="currentColor" stroke="none" /></svg>);
    case "arrowUpRight":
      return (<svg {...common}><path d="M7 17L17 7M8 7h9v9" /></svg>);
    case "arrowUp":
      return (<svg {...common}><path d="M12 19V5M6 11l6-6 6 6" /></svg>);
    case "arrowDown":
      return (<svg {...common}><path d="M12 5v14M6 13l6 6 6-6" /></svg>);
    case "check":
      return (<svg {...common}><path d="M5 12l4 4 10-10" /></svg>);
    case "clock":
      return (<svg {...common}><circle cx="12" cy="12" r="8" /><path d="M12 8v4l3 2" /></svg>);
    case "award":
      return (<svg {...common}><circle cx="12" cy="9" r="5" /><path d="M9 13l-2 7 5-3 5 3-2-7" /></svg>);
    case "run":
      return (<svg {...common}><circle cx="14" cy="5" r="2" /><path d="M5 20l3-5 3 1 1-4 3 3h3M11 12l-2-3" /></svg>);
    case "dot":
      return (<svg {...common}><circle cx="12" cy="12" r="8" /><circle cx="12" cy="12" r="3" fill="currentColor" stroke="none" /></svg>);
    case "replace":
      return (<svg {...common}><path d="M4 8h12l-3-3M20 16H8l3 3" /></svg>);
    case "ban":
      return (<svg {...common}><circle cx="12" cy="12" r="8" /><path d="M6.5 6.5l11 11" /></svg>);
    case "cardY":
      return (<svg {...common}><rect x="6" y="4" width="12" height="16" rx="2" /><text x="12" y="15" textAnchor="middle" fontSize="9" fill="currentColor" stroke="none">Y</text></svg>);
    case "cardR":
      return (<svg {...common}><rect x="6" y="4" width="12" height="16" rx="2" /><text x="12" y="15" textAnchor="middle" fontSize="9" fill="currentColor" stroke="none">R</text></svg>);
    default:
      return (<svg {...common}><circle cx="12" cy="12" r="8" /></svg>);
  }
}
