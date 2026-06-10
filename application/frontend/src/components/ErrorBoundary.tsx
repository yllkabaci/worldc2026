import { Component, type ErrorInfo, type ReactNode } from "react";
import { logger } from "../lib/logging/logger";

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}
interface State {
  hasError: boolean;
}

/** The only permitted class component (React error boundaries require one). */
export class AppErrorBoundary extends Component<Props, State> {
  state: State = { hasError: false };

  static getDerivedStateFromError(): State {
    return { hasError: true };
  }

  componentDidCatch(error: Error, info: ErrorInfo): void {
    logger.error("render_crash", { message: error.message, componentStack: info.componentStack });
  }

  render(): ReactNode {
    if (this.state.hasError) {
      return (
        this.props.fallback ?? (
          <div className="container">
            <h1>Something went wrong</h1>
            <p>Please reload the page.</p>
          </div>
        )
      );
    }
    return this.props.children;
  }
}
