import { screen } from "@testing-library/react";
import { http, HttpResponse } from "msw";
import App from "./App";
import { renderWithProviders } from "./test/render";
import { server } from "./test/server";
import { describe, expect, it } from "vitest";

describe("App error handling", () => {
  it("shows friendly error message when API fails", async () => {
    server.use(
      http.get("/api/customers/12345/profile", () => {
        return HttpResponse.json({ error: "boom" }, { status: 500 });
      }),
    );

    renderWithProviders(<App />);

    // match your improved wording
    expect(
      await screen.findByText(/We couldn’t load your spending data/i),
    ).toBeInTheDocument();
  });
});
