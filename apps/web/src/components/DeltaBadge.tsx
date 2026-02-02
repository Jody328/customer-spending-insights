type Props = { value: number };

export function DeltaBadge({ value }: Props) {
  const isUp = value >= 0;
  const cls = isUp
    ? "bg-emerald-100 text-emerald-800"
    : "bg-rose-100 text-rose-800";
  const sign = isUp ? "+" : "";

  return (
    <span className={`px-2 py-1 rounded-full text-xs font-semibold ${cls}`}>
      {sign}
      {value.toFixed(1)}%
    </span>
  );
}
