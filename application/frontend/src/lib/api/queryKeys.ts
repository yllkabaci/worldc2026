/** Central query-key factory. Never inline string arrays in hooks. */
export const queryKeys = {
  health: () => ["health"] as const,
  matches: {
    all: () => ["matches"] as const,
    calendar: () => ["matches", "calendar"] as const,
    detail: (id: string) => ["matches", "detail", id] as const,
  },
  predictions: {
    all: () => ["predictions"] as const,
    mine: () => ["predictions", "mine"] as const,
  },
  leaderboard: {
    all: () => ["leaderboard"] as const,
  },
} as const;
