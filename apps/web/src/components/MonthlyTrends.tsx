import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  CartesianGrid,
} from "recharts";
import type { SpendingTrendsResponse } from "../api/schemas";

function formatMoney(amount: number, currency: string) {
  return new Intl.NumberFormat("en-ZA", {
    style: "currency",
    currency,
    maximumFractionDigits: 0,
  }).format(amount);
}

function monthLabel(yyyyMm: string) {
  const [y, m] = yyyyMm.split("-").map(Number);
  const d = new Date(Date.UTC(y, m - 1, 1));
  return d.toLocaleString("en-ZA", { month: "short" });
}

export function MonthlyTrends({
  data,
  currency,
}: {
  data: SpendingTrendsResponse;
  currency: string;
}) {
  const chartData = data.trends.map((t) => ({
    month: monthLabel(t.month),
    totalSpent: t.totalSpent,
    transactionCount: t.transactionCount,
  }));

  const tooltipFormatter = (value: unknown, name: unknown) => {
    const n = typeof value === "number" ? value : Number(value);
    const label = typeof name === "string" ? name : "Total spent";

    if (!Number.isFinite(n)) return ["—", label] as const;
    return [formatMoney(n, currency), label] as const;
  };

  return (
    <div className="rounded-2xl border bg-white p-5 shadow-sm">
      <div className="flex items-start justify-between gap-4">
        <div>
          <div className="text-sm font-semibold text-slate-900">
            Monthly trends
          </div>
          <div className="mt-1 text-xs text-slate-500">
            Last {data.trends.length} months
          </div>
        </div>
      </div>

      <div className="mt-4 h-64">
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="month" />
            <YAxis
              tickFormatter={(v) => formatMoney(Number(v), currency)}
              width={90}
            />
            <Tooltip formatter={tooltipFormatter} />
            <Line
              type="monotone"
              dataKey="totalSpent"
              strokeWidth={2}
              dot={false}
            />
          </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
