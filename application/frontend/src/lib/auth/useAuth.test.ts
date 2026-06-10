import { describe, it, expect } from "vitest";
import { hasRoleHierarchy } from "./useAuth";

describe("hasRoleHierarchy", () => {
  it("Admin satisfies User", () => expect(hasRoleHierarchy(["Admin"], "User")).toBe(true));
  it("SuperAdmin satisfies Admin", () => expect(hasRoleHierarchy(["SuperAdmin"], "Admin")).toBe(true));
  it("User does not satisfy Admin", () => expect(hasRoleHierarchy(["User"], "Admin")).toBe(false));
  it("no roles satisfies nothing", () => expect(hasRoleHierarchy([], "User")).toBe(false));
});
