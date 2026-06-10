import { Link } from "react-router-dom";

export function NotFound() {
  return (
    <div className="container">
      <h1>404 — Not found</h1>
      <Link to="/">Go home</Link>
    </div>
  );
}
