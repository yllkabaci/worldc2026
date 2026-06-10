import { describe, it, expect, vi } from "vitest";
import { applyProblemDetailsToForm } from "./applyProblemDetailsToForm";
import type { ProblemDetails } from "../api/problemDetails";

describe("applyProblemDetailsToForm", () => {
  it("maps a validation errors dictionary onto fields (PascalCase → camelCase)", () => {
    // Arrange
    const setError = vi.fn();
    const problem: ProblemDetails = {
      title: "Validation failed",
      status: 400,
      errors: { Email: ["Email is invalid."], Password: ["Too short.", "No digit."] },
    };
    // Act
    applyProblemDetailsToForm(problem, setError);
    // Assert
    expect(setError).toHaveBeenCalledWith("email", { type: "server", message: "Email is invalid." });
    expect(setError).toHaveBeenCalledWith("password", { type: "server", message: "Too short. No digit." });
    expect(setError).not.toHaveBeenCalledWith("root", expect.anything());
  });

  it("falls back to a root error when there is no errors dictionary", () => {
    const setError = vi.fn();
    const problem: ProblemDetails = { title: "Conflict", status: 409, detail: "Email already exists." };

    applyProblemDetailsToForm(problem, setError);

    expect(setError).toHaveBeenCalledWith("root", { type: "server", message: "Email already exists." });
  });

  it("uses the title when no detail is present", () => {
    const setError = vi.fn();
    const problem: ProblemDetails = { title: "Server error", status: 500 };

    applyProblemDetailsToForm(problem, setError);

    expect(setError).toHaveBeenCalledWith("root", { type: "server", message: "Server error" });
  });
});
