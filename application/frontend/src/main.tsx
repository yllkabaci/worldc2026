import React from "react";
import ReactDOM from "react-dom/client";
import { AppProviders } from "./app/providers";
import { AppRouter } from "./app/router";
import "./lib/i18n";
import "./index.css";

const root = document.getElementById("root");
if (!root) throw new Error("Root element #root not found");

ReactDOM.createRoot(root).render(
  <React.StrictMode>
    <AppProviders>
      <AppRouter />
    </AppProviders>
  </React.StrictMode>,
);
