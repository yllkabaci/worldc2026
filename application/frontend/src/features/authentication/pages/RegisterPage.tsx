import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { registerSchema, type RegisterFormValues } from "../schemas/registerSchema";
import { useRegister } from "../api/useRegister";
import { applyProblemDetailsToForm } from "../../../lib/forms/applyProblemDetailsToForm";
import type { ProblemDetails } from "../../../lib/api/problemDetails";

export function RegisterPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const registerMutation = useRegister();
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormValues>({ resolver: zodResolver(registerSchema) });

  const onSubmit = handleSubmit(async ({ email, password }) => {
    try {
      await registerMutation.mutateAsync({ email, password });
      navigate("/login", { state: { justRegistered: true } });
    } catch (e) {
      const problem = e as ProblemDetails;
      if (problem.status === 409) {
        setError("email", { type: "server", message: t("auth.emailTaken") });
      } else {
        applyProblemDetailsToForm(problem, setError);
      }
    }
  });

  return (
    <main className="container">
      <h1>{t("auth.createAccount")}</h1>
      <form onSubmit={onSubmit} noValidate>
        {errors.root && (
          <p className="error" role="alert">
            {errors.root.message}
          </p>
        )}
        <div className="field">
          <label htmlFor="email">{t("auth.email")}</label>
          <input id="email" type="email" autoComplete="email" {...register("email")} />
          {errors.email && (
            <span className="error" role="alert">
              {errors.email.message}
            </span>
          )}
        </div>
        <div className="field">
          <label htmlFor="password">{t("auth.password")}</label>
          <input
            id="password"
            type="password"
            autoComplete="new-password"
            aria-describedby="password-hint"
            {...register("password")}
          />
          <span id="password-hint" className="hint">
            {t("auth.passwordHint")}
          </span>
          {errors.password && (
            <span className="error" role="alert">
              {errors.password.message}
            </span>
          )}
        </div>
        <div className="field">
          <label htmlFor="confirmPassword">{t("auth.confirmPassword")}</label>
          <input
            id="confirmPassword"
            type="password"
            autoComplete="new-password"
            {...register("confirmPassword")}
          />
          {errors.confirmPassword && (
            <span className="error" role="alert">
              {errors.confirmPassword.message}
            </span>
          )}
        </div>
        <button className="btn" type="submit" disabled={isSubmitting}>
          {t("auth.register")}
        </button>
      </form>
      <p>
        <Link to="/login">{t("auth.haveAccount")}</Link>
      </p>
    </main>
  );
}
