import { describe, it, expect } from "vitest";
import { registerSchema } from "./registerSchema";

const valid = {
  email: "jane@example.com",
  password: "Password1!",
  confirmPassword: "Password1!",
};

describe("registerSchema", () => {
  it("accepts a valid registration", () => {
    // Arrange / Act
    const result = registerSchema.safeParse(valid);
    // Assert
    expect(result.success).toBe(true);
  });

  it.each([
    ["not-an-email", "email"],
    ["", "email"],
  ])("rejects invalid email %s", (email, field) => {
    const result = registerSchema.safeParse({ ...valid, email });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues.some((i) => i.path[0] === field)).toBe(true);
    }
  });

  it.each([
    ["short1!", "too short (min 8)"],
    ["password1!", "no uppercase"],
    ["Password!!", "no digit"],
    ["Password11", "no special character"],
  ])("rejects password %s — %s", (password) => {
    const result = registerSchema.safeParse({ ...valid, password, confirmPassword: password });
    expect(result.success).toBe(false);
  });

  it("rejects when confirmPassword does not match", () => {
    const result = registerSchema.safeParse({ ...valid, confirmPassword: "Different1!" });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues.some((i) => i.path[0] === "confirmPassword")).toBe(true);
    }
  });
});
