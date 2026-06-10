import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";
import { useBackendHealth } from "../api/useBackendHealth";
import { Skeleton } from "../../../components/Skeleton";

export function HomePage() {
  const { t } = useTranslation();
  const { data, isLoading, isError } = useBackendHealth();

  return (
    <main className="container">
      <h1>{t("app.title")}</h1>
      <p>{t("home.welcome")}</p>
      <section aria-label={t("home.backendStatus")}>
        <h2>{t("home.backendStatus")}</h2>
        {isLoading ? (
          <Skeleton />
        ) : isError ? (
          <span className="status status--bad">{t("status.unreachable")}</span>
        ) : (
          <span className="status status--ok">
            {t("status.healthy")} — {data}
          </span>
        )}
      </section>
      <nav>
        <Link to="/login">{t("auth.login")}</Link>
        {" · "}
        <Link to="/register">{t("auth.register")}</Link>
      </nav>
    </main>
  );
}
