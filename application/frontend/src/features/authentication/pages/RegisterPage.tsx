import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { registerSchema, type RegisterFormValues } from "../schemas/registerSchema";
import { useRegister } from "../api/useRegister";
import { applyProblemDetailsToForm } from "../../../lib/forms/applyProblemDetailsToForm";
import type { ProblemDetails } from "../../../lib/api/problemDetails";
import "./auth-scene.css";

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
    <div className="auth-page">
      <div className="scene">
        <div className="bg" />
        <div className="lights" />
        <svg className="pitch-lines" viewBox="0 0 700 520" preserveAspectRatio="xMidYMid slice" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
          <rect x="60" y="40" width="400" height="440" fill="none" stroke="#fff" strokeWidth="1.5" />
          <line x1="60" y1="260" x2="460" y2="260" stroke="#fff" strokeWidth="1" />
          <circle cx="260" cy="260" r="70" fill="none" stroke="#fff" strokeWidth="1" />
          <circle cx="260" cy="260" r="3" fill="#fff" />
          <rect x="60" y="170" width="90" height="180" fill="none" stroke="#fff" strokeWidth="1" />
          <rect x="370" y="170" width="90" height="180" fill="none" stroke="#fff" strokeWidth="1" />
        </svg>

        <div className="left-content">
          <div className="brand">
            <div className="brand-icon" aria-hidden="true">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="#0a1a0a" strokeWidth="2">
                <circle cx="12" cy="12" r="9" />
                <path d="M12 7.5l3.2 2.3-1.2 3.7h-4l-1.2-3.7z" fill="#0a1a0a" stroke="none" />
              </svg>
            </div>
            <span className="brand-name">WC 2026 Predictions</span>
          </div>
          <div className="hero-text">
            <h1>Predict.<br /><span>Compete.</span><br />Win.</h1>
            <p>Make your match predictions for the biggest World Cup in history. 48 teams, 104 matches, one champion.</p>
          </div>
          <div className="stats-row">
            <div className="stat-chip"><strong>48</strong><span>Teams</span></div>
            <div className="stat-chip"><strong>104</strong><span>Matches</span></div>
            <div className="stat-chip"><strong>3</strong><span>Host nations</span></div>
          </div>
        </div>

        <form className="glass-card" onSubmit={onSubmit} noValidate>
          <p className="card-title">{t("auth.createAccount")}</p>
          <p className="card-sub">Join free and start predicting today</p>

          {errors.root && <p className="form-error" role="alert">{errors.root.message}</p>}

          <div className="field">
            <label htmlFor="email">{t("auth.email")}</label>
            <input id="email" type="email" placeholder="Enter your email" autoComplete="email"
              aria-invalid={!!errors.email} {...register("email")} />
            {errors.email && <span className="error" role="alert">{errors.email.message}</span>}
          </div>

          <div className="field">
            <label htmlFor="password">{t("auth.password")}</label>
            <input id="password" type="password" placeholder="Min. 8 characters" autoComplete="new-password"
              aria-describedby="password-hint" aria-invalid={!!errors.password} {...register("password")} />
            <span id="password-hint" className="hint">{t("auth.passwordHint")}</span>
            {errors.password && <span className="error" role="alert">{errors.password.message}</span>}
          </div>

          <div className="field">
            <label htmlFor="confirmPassword">{t("auth.confirmPassword")}</label>
            <input id="confirmPassword" type="password" placeholder="Re-enter your password" autoComplete="new-password"
              aria-invalid={!!errors.confirmPassword} {...register("confirmPassword")} />
            {errors.confirmPassword && <span className="error" role="alert">{errors.confirmPassword.message}</span>}
          </div>

          <button className="btn-main" type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Creating…" : t("auth.register")}
          </button>

          <p className="signin-link"><Link to="/login">{t("auth.haveAccount")}</Link></p>
        </form>
      </div>
    </div>
  );
}
