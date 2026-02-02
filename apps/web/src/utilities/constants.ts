export const PERIODS = [
  { label: "Last 7 days", value: "7d" },
  { label: "Last 30 days", value: "30d" },
  { label: "Last 90 days", value: "90d" },
  { label: "Last year", value: "1y" },
] as const;

export type PeriodValue = (typeof PERIODS)[number]["value"];
