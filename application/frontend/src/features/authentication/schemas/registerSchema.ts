import { z } from "zod";

/**
 * Mirrors the backend RegisterValidator (FluentValidation) so the user gets instant feedback.
 * The server remains the source of truth. `confirmPassword` is a client-only UX field and is
 * never sent to the API.
 */
export const registerSchema = z
  .object({
    email: z.string().min(1).email().max(256),
    password: z
      .string()
      .min(8, "Password must be at least 8 characters.")
      .regex(/[0-9]/, "Password must contain at least one digit.")
      .regex(/[A-Z]/, "Password must contain at least one uppercase letter.")
      .regex(/[^A-Za-z0-9]/, "Password must contain at least one special character."),
    confirmPassword: z.string().min(1),
  })
  .refine((v) => v.password === v.confirmPassword, {
    message: "Passwords do not match.",
    path: ["confirmPassword"],
  });

export type RegisterFormValues = z.infer<typeof registerSchema>;
