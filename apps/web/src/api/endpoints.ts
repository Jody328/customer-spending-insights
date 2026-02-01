import { z } from "zod";
import { getJson } from "./http";
import { CustomerProfileSchema, SpendingCategoriesResponseSchema, SpendingSummarySchema, SpendingTrendsResponseSchema } from "./schemas";
import { TransactionsResponseSchema } from "./schemas";

export type SortBy =
  | "date_desc"
  | "date_asc"
  | "amount_desc"
  | "amount_asc";

export type TransactionsQuery = {
  limit?: number;
  offset?: number;
  category?: string;
  startDate?: string; // YYYY-MM-DD
  endDate?: string;   // YYYY-MM-DD
  period?: string;    // 7d | 30d | 90d | 1y
  sortBy?: SortBy;
};

function toQueryString(q: TransactionsQuery) {
  const params = new URLSearchParams();

  const set = (key: string, value: unknown) => {
    if (value === undefined || value === null) return;
    if (typeof value === "string" && value.trim() === "") return;
    params.set(key, String(value));
  };

  set("limit", q.limit);
  set("offset", q.offset);
  set("category", q.category);
  set("startDate", q.startDate);
  set("endDate", q.endDate);
  set("period", q.period);
  set("sortBy", q.sortBy);

  const s = params.toString();
  return s ? `?${s}` : "";
}


export async function fetchTransactions(customerId: string, q: TransactionsQuery) {
  const data = await getJson<unknown>(
    `/api/customers/${customerId}/transactions${toQueryString(q)}`
  );
  return parseWith(TransactionsResponseSchema, data);
}

function parseWith<T>(schema: z.ZodType<T>, data: unknown): T {
  const result = schema.safeParse(data);
  if (!result.success) {
    throw new Error(`API response validation failed: ${result.error.message}`);
  }
  return result.data;
}

export async function fetchCustomerProfile(customerId: string) {
  const data = await getJson<unknown>(`/api/customers/${customerId}/profile`);
  return parseWith(CustomerProfileSchema, data);
}

export async function fetchSpendingSummary(customerId: string, period: string) {
  const data = await getJson<unknown>(
    `/api/customers/${customerId}/spending/summary?period=${encodeURIComponent(period)}`
  );
  return parseWith(SpendingSummarySchema, data);
}

export async function fetchSpendingCategories(customerId: string, period: string) {
  const data = await getJson<unknown>(
    `/api/customers/${customerId}/spending/categories?period=${encodeURIComponent(period)}`
  );
  return parseWith(SpendingCategoriesResponseSchema, data);
}

export async function fetchSpendingTrends(customerId: string, months = 12) {
  const data = await getJson<unknown>(
    `/api/customers/${customerId}/spending/trends?months=${months}`
  );
  return parseWith(SpendingTrendsResponseSchema, data);
}