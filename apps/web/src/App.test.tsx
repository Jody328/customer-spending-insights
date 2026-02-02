import { screen } from "@testing-library/react";
import App from "./App";
import { renderWithProviders } from "./test/render";
import { describe, expect, it, vi } from "vitest";

vi.mock("./components/CategoryDonut", () => ({
  CategoryDonut: () => <div data-testid="category-donut" />,
}));

vi.mock("./components/MonthlyTrends", () => ({
  MonthlyTrends: () => <div data-testid="monthly-trends" />,
}));

describe("App", () => {
  it("renders dashboard title and customer details", async () => {
    renderWithProviders(<App />);

    expect(screen.getByText(/Customer Spending Insights/i)).toBeInTheDocument();

    // waits for async react-query data
    expect(await screen.findByText(/John Doe/i)).toBeInTheDocument();
    expect(screen.getByText(/premium/i)).toBeInTheDocument();
  });

  it("renders transactions table", async () => {
    renderWithProviders(<App />);

    expect(await screen.findByText(/Merchant/i)).toBeInTheDocument();
    // one of the mocked merchants
    expect(await screen.findByText(/Pick n Pay/i)).toBeInTheDocument();
  });
});
