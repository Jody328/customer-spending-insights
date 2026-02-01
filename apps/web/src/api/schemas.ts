import { z } from "zod";

export const TransactionSchema = z.object({
  id: z.string(),
  date: z.string(), // ISO string
  merchant: z.string(),
  category: z.string(),
  amount: z.number(),
  description: z.string(),
  paymentMethod: z.string(),
  icon: z.string().optional(),
  categoryColor: z.string().optional(),
});

export const PaginationSchema = z.object({
  total: z.number(),
  limit: z.number(),
  offset: z.number(),
  hasMore: z.boolean(),
});

export const TransactionsResponseSchema = z.object({
  transactions: z.array(TransactionSchema),
  pagination: PaginationSchema,
});

export type Transaction = z.infer<typeof TransactionSchema>;
export type TransactionsResponse = z.infer<typeof TransactionsResponseSchema>;

export const CustomerProfileSchema = z.object({
  customerId: z.string(),
  name: z.string(),
  email: z.string().email(),
  joinDate: z.string(), // ISO date string
  accountType: z.string(),
  totalSpent: z.number(),
  currency: z.string(),
});
export type CustomerProfile = z.infer<typeof CustomerProfileSchema>;

export const SpendingSummarySchema = z.object({
  period: z.string(),
  totalSpent: z.number(),
  transactionCount: z.number(),
  averageTransaction: z.number(),
  topCategory: z.string(),
  comparedToPrevious: z.object({
    spentChange: z.number(),
    transactionChange: z.number(),
  }),
});
export type SpendingSummary = z.infer<typeof SpendingSummarySchema>;

export const SpendingCategoryItemSchema = z.object({
  name: z.string(),
  amount: z.number(),
  percentage: z.number(),
  transactionCount: z.number(),
  color: z.string(),
  icon: z.string().optional(),
});

export const DateRangeSchema = z.object({
  startDate: z.string(),
  endDate: z.string(),
});

export const SpendingCategoriesResponseSchema = z.object({
  dateRange: DateRangeSchema,
  totalAmount: z.number(),
  categories: z.array(SpendingCategoryItemSchema),
});

export type SpendingCategoriesResponse = z.infer<typeof SpendingCategoriesResponseSchema>;

export const TrendItemSchema = z.object({
  month: z.string(), // YYYY-MM
  totalSpent: z.number(),
  transactionCount: z.number(),
  averageTransaction: z.number(),
});

export const SpendingTrendsResponseSchema = z.object({
  trends: z.array(TrendItemSchema),
});

export type SpendingTrendsResponse = z.infer<typeof SpendingTrendsResponseSchema>;