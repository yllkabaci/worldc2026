import { describe, it, expect } from "vitest";
import { roleMatches, landingPathForRoles } from "./useAuth";

describe("roleMatches", () => {
  it("Admin role satisfies Admin", () => expect(roleMatches(["Admin"], "Admin")).toBe(true));
  it("Admin satisfies User", () => expect(roleMatches(["Admin"], "User")).toBe(true));
  it("User does not satisfy Admin", () => expect(roleMatches(["User"], "Admin")).toBe(false));
  it("authenticated User satisfies User", () => expect(roleMatches(["User"], "User")).toBe(true));
  it("no roles satisfies nothing", () => expect(roleMatches([], "User")).toBe(false));
});

describe("landingPathForRoles", () => {
  it("sends admins to /admin", () => expect(landingPathForRoles(["Admin"])).toBe("/admin"));
  it("sends regular users to /dashboard", () => expect(landingPathForRoles(["User"])).toBe("/dashboard"));
  it("defaults to /dashboard when no roles", () => expect(landingPathForRoles([])).toBe("/dashboard"));
});
