import { http, HttpResponse } from "msw";

const customerId = "12345";

export const handlers = [
  // Profile
  http.get(`/api/customers/${customerId}/profile`, () => {
    return HttpResponse.json({
      customerId,
      name: "John Doe",
      email: "john.doe@email.com",
      joinDate: "2023-01-15",
      accountType: "premium",
      totalSpent: 15420.5,
      currency: "ZAR",
    });
  }),

  // Summary
  http.get(`/api/customers/${customerId}/spending/summary`, ({ request }) => {
    const url = new URL(request.url);
    const period = url.searchParams.get("period") ?? "30d";

    return HttpResponse.json({
      period,
      totalSpent: 4250.75,
      transactionCount: 47,
      averageTransaction: 90.44,
      topCategory: "Groceries",
      comparedToPrevious: {
        spentChange: 12.5,
        transactionChange: -3.2,
      },
    });
  }),

  // Categories
  http.get(`/api/customers/${customerId}/spending/categories`, () => {
    return HttpResponse.json({
      dateRange: { startDate: "2024-08-16", endDate: "2024-09-16" },
      totalAmount: 4250.75,
      categories: [
        {
          name: "Groceries",
          amount: 1250.3,
          percentage: 29.4,
          transactionCount: 15,
          color: "#FF6B6B",
          icon: "shopping-cart",
        },
      ],
    });
  }),

  // Trends
  http.get(`/api/customers/${customerId}/spending/trends`, () => {
    return HttpResponse.json({
      trends: [
        {
          month: "2024-01",
          totalSpent: 3890.25,
          transactionCount: 42,
          averageTransaction: 92.62,
        },
      ],
    });
  }),

  // Transactions
  http.get(`/api/customers/${customerId}/transactions`, ({ request }) => {
    const url = new URL(request.url);

    // Simulate simple filtering behaviour
    const category = url.searchParams.get("category");
    const limit = Number(url.searchParams.get("limit") ?? 20);
    const offset = Number(url.searchParams.get("offset") ?? 0);

    const all = [
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
      {
        id: "txn_2",
        date: "2024-09-15T10:15:00Z",
        merchant: "Netflix",
        category: "Entertainment",
        amount: 199.0,
        description: "Monthly subscription",
        paymentMethod: "Debit Order",
        icon: "film",
        categoryColor: "#4ECDC4",
      },
    ];

    const filtered = category
      ? all.filter((t) => t.category === category)
      : all;

    const page = filtered.slice(offset, offset + limit);

    return HttpResponse.json({
      transactions: page,
      pagination: {
        total: filtered.length,
        limit,
        offset,
        hasMore: offset + limit < filtered.length,
      },
    });
  }),
];
