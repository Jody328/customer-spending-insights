import { Skeleton } from "../Skeleton";

export function CardSkeleton({
  titleWidth = "w-40",
  height = "h-64",
}: {
  titleWidth?: string;
  height?: string;
}) {
  return (
    <div className="rounded-2xl border bg-white p-5 shadow-sm">
      <div className="flex items-start justify-between gap-4">
        <Skeleton className={`h-4 ${titleWidth}`} />
        <Skeleton className="h-4 w-20" />
      </div>
      <Skeleton className="mt-2 h-3 w-44" />
      <div className={`mt-5 ${height}`}>
        <Skeleton className="h-full w-full" />
      </div>
    </div>
  );
}
