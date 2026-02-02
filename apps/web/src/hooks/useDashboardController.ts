import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";

import {
  fetchCustomerProfile,
  fetchSpendingCategories,
  fetchSpendingSummary,
  fetchSpendingTrends,
  fetchTransactions,
  type SortBy,
} from "../api/endpoints";

import { PERIODS, type PeriodValue } from "../utilities/constants";
import { formatMoney } from "../utilities/format";

type TxChange = {
  category?: string;
  startDate?: string;
  endDate?: string;
  sortBy?: SortBy;
  limit?: number;
  offset?: number;
};

export function useDashboardController(customerId: string) {
  const [period, setPeriod] = useState<PeriodValue>("30d");

  const [txCategory, setTxCategory] = useState("");
  const [txStartDate, setTxStartDate] = useState("");
  const [txEndDate, setTxEndDate] = useState("");
  const [txSortBy, setTxSortBy] = useState<SortBy>("date_desc");
  const [txLimit, setTxLimit] = useState(20);
  const [txOffset, setTxOffset] = useState(0);

  const profileQuery = useQuery({
    queryKey: ["customerProfile", customerId],
    queryFn: () => fetchCustomerProfile(customerId),
  });

  const summaryQuery = useQuery({
    queryKey: ["spendingSummary", customerId, period],
    queryFn: () => fetchSpendingSummary(customerId, period),
    enabled: true,
  });

  const categoriesQuery = useQuery({
    queryKey: ["spendingCategories", customerId, period],
    queryFn: () => fetchSpendingCategories(customerId, period),
  });

  const trendsQuery = useQuery({
    queryKey: ["spendingTrends", customerId],
    queryFn: () => fetchSpendingTrends(customerId, 12),
  });

  const txQuery = useQuery({
    queryKey: [
      "transactions",
      customerId,
      period,
      txCategory,
      txStartDate,
      txEndDate,
      txSortBy,
      txLimit,
      txOffset,
    ],
    queryFn: () =>
      fetchTransactions(customerId, {
        period,
        category: txCategory || undefined,
        startDate: txStartDate || undefined,
        endDate: txEndDate || undefined,
        sortBy: txSortBy,
        limit: txLimit,
        offset: txOffset,
      }),
  });

  const currency = profileQuery.data?.currency ?? "ZAR";

  const error =
    profileQuery.error ||
    summaryQuery.error ||
    categoriesQuery.error ||
    trendsQuery.error;

  const cards = useMemo(() => {
    const s = summaryQuery.data;
    if (!s) return [] as Array<{ title: string; value: string; delta?: number }>;

    return [
      {
        title: "Total spent",
        value: formatMoney(s.totalSpent, currency),
        delta: s.comparedToPrevious.spentChange,
      },
      {
        title: "Transactions",
        value: s.transactionCount.toString(),
        delta: s.comparedToPrevious.transactionChange,
      },
      {
        title: "Avg. transaction",
        value: formatMoney(s.averageTransaction, currency),
      },
      {
        title: "Top category",
        value: s.topCategory || "—",
      },
    ];
  }, [summaryQuery.data, currency]);

  // When changing period, reset transaction paging so we don't land on an empty page.
  const handlePeriodChange = (nextPeriod: PeriodValue) => {
    setPeriod(nextPeriod);
    setTxOffset(0);
  };

  const handleTxChange = (next: TxChange) => {
    if (next.category !== undefined) setTxCategory(next.category);
    if (next.startDate !== undefined) setTxStartDate(next.startDate);
    if (next.endDate !== undefined) setTxEndDate(next.endDate);
    if (next.sortBy !== undefined) setTxSortBy(next.sortBy);
    if (next.limit !== undefined) setTxLimit(next.limit);
    if (next.offset !== undefined) setTxOffset(next.offset);
  };

  return {
    PERIODS,
    period,
    handlePeriodChange,

    txState: {
      category: txCategory,
      startDate: txStartDate,
      endDate: txEndDate,
      sortBy: txSortBy,
      limit: txLimit,
      offset: txOffset,
    },
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
  };
}
