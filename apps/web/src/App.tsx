import { DEFAULT_CUSTOMER_ID } from "./api/config";
import { CategoryDonut } from "./components/CategoryDonut";
import { MonthlyTrends } from "./components/MonthlyTrends";
import { TransactionsTable } from "./components/TransactionsTable";
import { KpiRowSkeleton } from "./components/skeletons/KpiRowSkeleton";
import { CardSkeleton } from "./components/skeletons/CardSkeleton";
import { TableSkeleton } from "./components/skeletons/TableSkeleton";
import { DeltaBadge } from "./components/DeltaBadge";
import { formatMoney } from "./utilities/format";
import { useDashboardController } from "./hooks/useDashboardController";
import type { PeriodValue } from "./utilities/constants";

export default function App() {
  const customerId = DEFAULT_CUSTOMER_ID;

  const {
    PERIODS,
    period,
    handlePeriodChange,
    txState,
    handleTxChange,
    currency,
    cards,
    error,
    queries: {
      profileQuery,
      summaryQuery,
      categoriesQuery,
      trendsQuery,
      txQuery,
    },
  } = useDashboardController(customerId);

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="border-b bg-white">
        <div className="mx-auto w-full max-w-screen-2xl px-6 py-5 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="min-w-0">
            <h1 className="text-xl font-semibold text-slate-900">
              Customer Spending Insights
            </h1>
            <p className="text-sm text-slate-600">
              {profileQuery.data
                ? `${profileQuery.data.name} • ${profileQuery.data.accountType}`
                : profileQuery.isLoading
                  ? "Loading customer…"
                  : "Customer"}
            </p>
          </div>

          <div className="flex items-center gap-2 sm:justify-end">
            <label className="text-sm text-slate-600">Period</label>
            <select
              className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm w-full sm:w-auto"
              value={period}
              onChange={(e) =>
                handlePeriodChange(e.target.value as PeriodValue)
              }
            >
              {PERIODS.map((p) => (
                <option key={p.value} value={p.value}>
                  {p.label}
                </option>
              ))}
            </select>
          </div>
        </div>
      </header>

      <main className="mx-auto w-full max-w-screen-2xl px-6 py-6">
        {error && (
          <div className="mx-auto max-w-2xl rounded-2xl border border-rose-200 bg-rose-50 p-6 shadow-sm">
            <div className="flex items-start gap-4">
              <div className="flex h-10 w-10 items-center justify-center rounded-full bg-rose-100 text-rose-600">
                ⚠️
              </div>
              <div>
                <h3 className="text-lg font-semibold text-rose-900">
                  We couldn’t load your spending data
                </h3>

                <p className="mt-1 text-sm text-rose-800">
                  This usually happens when the service is temporarily
                  unavailable or your connection dropped.
                </p>

                <p className="mt-3 text-sm text-rose-700">
                  Please refresh the page or try again in a few moments.
                </p>

                <button
                  onClick={() => window.location.reload()}
                  className="mt-4 inline-flex items-center rounded-lg bg-rose-600 px-4 py-2 text-sm font-medium text-white hover:bg-rose-700 transition"
                >
                  Retry
                </button>
              </div>
            </div>
          </div>
        )}

        {!error && (
          <>
            {/* KPI cards */}
            <section className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
              {summaryQuery.isLoading ? (
                <KpiRowSkeleton />
              ) : (
                cards.map((c) => (
                  <div
                    key={c.title}
                    className="rounded-2xl border bg-white p-5 shadow-sm"
                  >
                    <div className="flex items-center justify-between gap-3">
                      <div className="text-sm text-slate-600">{c.title}</div>
                      {"delta" in c && typeof c.delta === "number" ? (
                        <DeltaBadge value={c.delta} />
                      ) : null}
                    </div>
                    <div className="mt-2 text-2xl font-semibold text-slate-900">
                      {c.value}
                    </div>
                  </div>
                ))
              )}
            </section>

            {/* Customer details */}
            <section className="mt-6 rounded-2xl border bg-white p-5 shadow-sm">
              <div className="text-sm font-semibold text-slate-900">
                Customer details
              </div>

              {profileQuery.isLoading ? (
                <div className="mt-3 grid grid-cols-1 gap-2 md:grid-cols-3">
                  <div className="h-5 w-56 animate-pulse rounded-lg bg-slate-200/70" />
                  <div className="h-5 w-40 animate-pulse rounded-lg bg-slate-200/70" />
                  <div className="h-5 w-52 animate-pulse rounded-lg bg-slate-200/70" />
                </div>
              ) : (
                <div className="mt-3 grid grid-cols-1 gap-2 text-sm text-slate-700 md:grid-cols-3">
                  <div>
                    <span className="text-slate-500">Email:</span>{" "}
                    {profileQuery.data?.email}
                  </div>
                  <div>
                    <span className="text-slate-500">Joined:</span>{" "}
                    {profileQuery.data?.joinDate}
                  </div>
                  <div>
                    <span className="text-slate-500">Lifetime spend:</span>{" "}
                    {profileQuery.data
                      ? formatMoney(profileQuery.data.totalSpent, currency)
                      : "—"}
                  </div>
                </div>
              )}
            </section>

            {/* Charts */}
            <section className="mt-6 grid grid-cols-1 gap-6 lg:grid-cols-2">
              {categoriesQuery.isLoading ? (
                <CardSkeleton titleWidth="w-44" height="h-56" />
              ) : categoriesQuery.data ? (
                <CategoryDonut
                  data={categoriesQuery.data}
                  currency={currency}
                />
              ) : null}

              {trendsQuery.isLoading ? (
                <CardSkeleton titleWidth="w-32" height="h-64" />
              ) : trendsQuery.data ? (
                <MonthlyTrends data={trendsQuery.data} currency={currency} />
              ) : null}
            </section>

            {/* Transactions */}
            <section className="mt-6">
              {txQuery.isLoading ? (
                <TableSkeleton rows={8} />
              ) : txQuery.data ? (
                <TransactionsTable
                  rows={txQuery.data.transactions}
                  currency={currency}
                  total={txQuery.data.pagination.total}
                  hasMore={txQuery.data.pagination.hasMore}
                  limit={txQuery.data.pagination.limit}
                  offset={txQuery.data.pagination.offset}
                  category={txState.category}
                  startDate={txState.startDate}
                  endDate={txState.endDate}
                  sortBy={txState.sortBy}
                  categories={
                    categoriesQuery.data?.categories.map((c) => c.name) ?? []
                  }
                  onChange={handleTxChange}
                />
              ) : null}

              {txQuery.isError && (
                <div className="mb-4 rounded-xl border border-rose-200 bg-rose-50 p-4 text-rose-900">
                  <div className="font-semibold">
                    Couldn’t load transactions
                  </div>
                  <div className="mt-1 text-sm text-rose-800">
                    Please check your filters (especially date range) and try
                    again.
                  </div>
                </div>
              )}
            </section>
          </>
        )}
      </main>
    </div>
  );
}
