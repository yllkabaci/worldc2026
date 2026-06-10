import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { loginSchema, type LoginFormValues } from "../schemas/loginSchema";
import { useLogin } from "../api/useLogin";
import type { ProblemDetails } from "../../../lib/api/problemDetails";

export function LoginPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const login = useLogin();
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) });

  const onSubmit = handleSubmit(async (values) => {
    try {
      await login.mutateAsync(values);
      navigate("/dashboard");
    } catch (e) {
      const problem = e as ProblemDetails;
      setError("password", { message: problem.detail ?? "Login failed" });
    }
  });

  return (
    <main className="container">
      <h1>{t("auth.login")}</h1>
      <form onSubmit={onSubmit} noValidate>
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
          <input id="password" type="password" autoComplete="current-password" {...register("password")} />
          {errors.password && (
            <span className="error" role="alert">
              {errors.password.message}
            </span>
          )}
        </div>
        <button className="btn" type="submit" disabled={isSubmitting}>
          {t("auth.login")}
        </button>
      </form>
    </main>
  );
}
