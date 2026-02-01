import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip } from "recharts";
import type { SpendingCategoriesResponse } from "../api/schemas";

function formatMoney(amount: number, currency: string) {
  return new Intl.NumberFormat("en-ZA", {
    style: "currency",
    currency,
    maximumFractionDigits: 2,
  }).format(amount);
}

export function CategoryDonut({
  data,
  currency,
}: {
  data: SpendingCategoriesResponse;
  currency: string;
}) {
  const chartData = data.categories.map((c) => ({
    name: c.name,
    value: c.amount,
    color: c.color,
    percentage: c.percentage,
    transactionCount: c.transactionCount,
  }));

  const tooltipFormatter = (value: unknown) => {
    const n = typeof value === "number" ? value : Number(value);
    if (!Number.isFinite(n)) return ["—", "Amount"] as const;
    return [formatMoney(n, currency), "Amount"] as const;
  };

  return (
    <div className="rounded-2xl border bg-white p-5 shadow-sm">
      <div className="flex items-start justify-between gap-4">
        <div>
          <div className="text-sm font-semibold text-slate-900">
            Spending by category
          </div>
          <div className="mt-1 text-xs text-slate-500">
            {data.dateRange.startDate} → {data.dateRange.endDate}
          </div>
        </div>
        <div className="text-sm font-semibold text-slate-900">
          {formatMoney(data.totalAmount, currency)}
        </div>
      </div>

      <div className="mt-4 grid grid-cols-1 gap-6 md:grid-cols-2">
        <div className="h-56">
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie
                data={chartData}
                dataKey="value"
                nameKey="name"
                innerRadius={55}
                outerRadius={85}
                paddingAngle={2}
              >
                {chartData.map((entry) => (
                  <Cell key={entry.name} fill={entry.color} />
                ))}
              </Pie>
              <Tooltip formatter={tooltipFormatter} />
            </PieChart>
          </ResponsiveContainer>
        </div>

        <div className="space-y-3">
          {data.categories
            .slice()
            .sort((a, b) => b.amount - a.amount)
            .map((c) => (
              <div
                key={c.name}
                className="flex items-center justify-between gap-3"
              >
                <div className="flex items-center gap-3">
                  <span
                    className="h-3 w-3 rounded-full"
                    style={{ backgroundColor: c.color }}
                  />
                  <div>
                    <div className="text-sm font-medium text-slate-900">
                      {c.name}
                    </div>
                    <div className="text-xs text-slate-500">
                      {c.transactionCount} tx • {c.percentage.toFixed(1)}%
                    </div>
                  </div>
                </div>
                <div className="text-sm font-semibold text-slate-900">
                  {formatMoney(c.amount, currency)}
                </div>
              </div>
            ))}
        </div>
      </div>
    </div>
  );
}
