import { useMemo } from "react";
import type { Transaction } from "../api/schemas";
import type { SortBy } from "../api/endpoints";

function formatMoney(amount: number, currency: string) {
  return new Intl.NumberFormat("en-ZA", {
    style: "currency",
    currency,
    maximumFractionDigits: 2,
  }).format(amount);
}

function formatDate(iso: string) {
  const d = new Date(iso);
  return d.toLocaleString("en-ZA", {
    year: "numeric",
    month: "short",
    day: "2-digit",
  });
}

export function TransactionsTable({
  rows,
  currency,
  limit,
  offset,
  total,
  hasMore,
  category,
  startDate,
  endDate,
  sortBy,
  onChange,
  categories,
}: {
  rows: Transaction[];
  currency: string;
  limit: number;
  offset: number;
  total: number;
  hasMore: boolean;

  category: string;
  startDate: string;
  endDate: string;
  sortBy: SortBy;

  categories: string[];
  onChange: (next: {
    category?: string;
    startDate?: string;
    endDate?: string;
    sortBy?: SortBy;
    offset?: number;
    limit?: number;
  }) => void;
}) {
  const page = Math.floor(offset / limit) + 1;
  const totalPages = Math.max(1, Math.ceil(total / limit));

  const showingText = useMemo(() => {
    const from = total === 0 ? 0 : offset + 1;
    const to = Math.min(offset + limit, total);
    return `Showing ${from}-${to} of ${total}`;
  }, [offset, limit, total]);

  return (
    <div className="rounded-2xl border bg-white p-5 shadow-sm">
      <div className="flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
        <div>
          <div className="text-sm font-semibold text-slate-900">
            Transactions
          </div>
          <div className="mt-1 text-xs text-slate-500">{showingText}</div>
        </div>

        <div className="grid grid-cols-2 gap-3 md:flex md:flex-wrap md:items-end">
          <div className="flex flex-col gap-1">
            <label className="text-xs text-slate-600">Category</label>
            <select
              className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
              value={category}
              onChange={(e) =>
                onChange({ category: e.target.value, offset: 0 })
              }
            >
              <option value="">All</option>
              {categories.map((c) => (
                <option key={c} value={c}>
                  {c}
                </option>
              ))}
            </select>
          </div>

          <div className="flex flex-col gap-1">
            <label className="text-xs text-slate-600">Sort</label>
            <select
              className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
              value={sortBy}
              onChange={(e) =>
                onChange({ sortBy: e.target.value as SortBy, offset: 0 })
              }
            >
              <option value="date_desc">Date (newest)</option>
              <option value="date_asc">Date (oldest)</option>
              <option value="amount_desc">Amount (high → low)</option>
              <option value="amount_asc">Amount (low → high)</option>
            </select>
          </div>

          <div className="flex flex-col gap-1">
            <label className="text-xs text-slate-600">Start date</label>
            <input
              type="date"
              className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
              max={endDate || undefined}
              value={startDate}
              onChange={(e) => {
                const nextStart = e.target.value;
                if (!endDate && nextStart) {
                  onChange({
                    startDate: nextStart,
                    endDate: nextStart,
                    offset: 0,
                  });
                  return;
                }

                onChange({ startDate: nextStart, offset: 0 });
              }}
            />
          </div>

          <div className="flex flex-col gap-1">
            <label className="text-xs text-slate-600">End date</label>
            <input
              type="date"
              className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm"
              min={startDate || undefined}
              value={endDate}
              onChange={(e) => {
                const nextEnd = e.target.value;

                // If startDate exists and nextEnd is before it, move startDate down to match.
                if (startDate && nextEnd && nextEnd < startDate) {
                  onChange({ endDate: nextEnd, startDate: nextEnd, offset: 0 });
                  return;
                }

                onChange({ endDate: nextEnd, offset: 0 });
              }}
            />
          </div>

          <button
            className="col-span-2 md:col-span-1 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm hover:bg-slate-50"
            onClick={() =>
              onChange({
                category: "",
                startDate: "",
                endDate: "",
                sortBy: "date_desc",
                offset: 0,
              })
            }
          >
            Reset
          </button>
        </div>
      </div>

      <div className="mt-4 overflow-x-auto">
        <table className="w-full min-w-[820px] text-sm">
          <thead>
            <tr className="border-b text-left text-slate-600">
              <th className="py-3 pr-4">Date</th>
              <th className="py-3 pr-4">Merchant</th>
              <th className="py-3 pr-4">Category</th>
              <th className="py-3 pr-4">Payment</th>
              <th className="py-3 pr-4">Description</th>
              <th className="py-3 text-right">Amount</th>
            </tr>
          </thead>

          <tbody className="divide-y">
            {rows.map((t) => (
              <tr key={t.id} className="text-slate-800">
                <td className="py-3 pr-4 whitespace-nowrap">
                  {formatDate(t.date)}
                </td>
                <td className="py-3 pr-4">{t.merchant}</td>
                <td className="py-3 pr-4">
                  <span
                    className="inline-flex items-center gap-2 rounded-full px-2 py-1 text-xs font-medium"
                    style={{
                      backgroundColor: t.categoryColor
                        ? `${t.categoryColor}20`
                        : "#E2E8F0",
                      color: t.categoryColor ?? "#0F172A",
                    }}
                  >
                    <span
                      className="h-2 w-2 rounded-full"
                      style={{ backgroundColor: t.categoryColor ?? "#64748B" }}
                    />
                    {t.category}
                  </span>
                </td>
                <td className="py-3 pr-4">{t.paymentMethod}</td>
                <td className="py-3 pr-4 text-slate-600">{t.description}</td>
                <td className="py-3 text-right font-semibold">
                  {formatMoney(t.amount, currency)}
                </td>
              </tr>
            ))}

            {rows.length === 0 && (
              <tr>
                <td className="py-10 text-center text-slate-500" colSpan={6}>
                  No transactions match the selected filters.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      <div className="mt-4 flex items-center justify-between gap-3">
        <div className="text-xs text-slate-500">
          Page {page} of {totalPages}
        </div>

        <div className="flex items-center gap-2">
          <button
            className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm disabled:opacity-50"
            disabled={offset === 0}
            onClick={() => onChange({ offset: Math.max(0, offset - limit) })}
          >
            Previous
          </button>

          <button
            className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm disabled:opacity-50"
            disabled={!hasMore}
            onClick={() => onChange({ offset: offset + limit })}
          >
            Next
          </button>

          <select
            className="rounded-lg border border-slate-300 bg-white px-2 py-2 text-sm"
            value={limit}
            onChange={(e) =>
              onChange({ limit: Number(e.target.value), offset: 0 })
            }
          >
            {[10, 20, 50, 100].map((n) => (
              <option key={n} value={n}>
                {n}/page
              </option>
            ))}
          </select>
        </div>
      </div>
    </div>
  );
}
