export function KpiRowSkeleton() {
  return (
    <>
      {[1, 2, 3, 4].map((i) => (
        <div key={i} className="rounded-2xl border bg-white p-5 shadow-sm">
          <div className="flex items-center justify-between">
            <div className="h-4 w-24 rounded bg-slate-200/70 animate-pulse" />
            <div className="h-5 w-12 rounded-full bg-slate-200/70 animate-pulse" />
          </div>

          <div className="mt-3 h-8 w-32 rounded bg-slate-200/70 animate-pulse" />
        </div>
      ))}
    </>
  );
}
