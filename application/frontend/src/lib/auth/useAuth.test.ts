import { describe, it, expect } from "vitest";
import { roleMatches } from "./useAuth";

describe("roleMatches", () => {
  it("Admin role satisfies Admin", () => expect(roleMatches(["Admin"], "Admin")).toBe(true));
  it("Admin satisfies User", () => expect(roleMatches(["Admin"], "User")).toBe(true));
  it("User does not satisfy Admin", () => expect(roleMatches(["User"], "Admin")).toBe(false));
  it("authenticated User satisfies User", () => expect(roleMatches(["User"], "User")).toBe(true));
  it("no roles satisfies nothing", () => expect(roleMatches([], "User")).toBe(false));
});
