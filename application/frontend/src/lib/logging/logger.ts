/** Structured UI logging. Never log tokens, claims, passwords, or PII. */
export const logger = {
  event(name: string, props: Record<string, unknown> = {}): void {
    if (import.meta.env.DEV) {
      // eslint-disable-next-line no-console
      console.info(`[event] ${name}`, props);
    }
  },
  error(name: string, props: Record<string, unknown> = {}): void {
    // eslint-disable-next-line no-console
    console.error(`[error] ${name}`, props);
  },
};
