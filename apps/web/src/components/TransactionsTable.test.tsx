import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { TransactionsTable } from "./TransactionsTable";
import { describe, expect, it, vi } from "vitest";

const baseProps = {
  rows: [
    {
      id: "txn_1",
      date: "2024-09-16T14:30:00Z",
      merchant: "Pick n Pay",
      category: "Groceries",
      amount: 245.8,
      description: "Weekly groceries",
      paymentMethod: "Credit Card",
      icon: "shopping-cart",
      categoryColor: "#FF6B6B",
    },
  ],
  currency: "ZAR",
  limit: 20,
  offset: 0,
  total: 1,
  hasMore: false,
  category: "",
  startDate: "",
  endDate: "",
  sortBy: "date_desc" as const,
  categories: ["Groceries", "Entertainment"],
  onChange: vi.fn(),
};

describe("TransactionsTable", () => {
  it("emits filter changes", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    render(<TransactionsTable {...baseProps} onChange={onChange} />);

    await user.selectOptions(
      screen.getByRole("combobox", { name: /Category/i }),
      "Groceries",
    );
    expect(onChange).toHaveBeenCalledWith(
      expect.objectContaining({ category: "Groceries", offset: 0 }),
    );
  });

  it("reset button clears filters", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    render(
      <TransactionsTable
        {...baseProps}
        category="Groceries"
        startDate="2024-09-01"
        endDate="2024-09-10"
        onChange={onChange}
      />,
    );

    await user.click(screen.getByRole("button", { name: /Reset/i }));
    expect(onChange).toHaveBeenCalledWith({
      category: "",
      startDate: "",
      endDate: "",
      sortBy: "date_desc",
      offset: 0,
    });
  });
});
