export function Skeleton({ count = 1 }: { count?: number }) {
  return (
    <>
      {Array.from({ length: count }).map((_, i) => (
        <span key={i} className="skeleton" aria-hidden="true" />
      ))}
    </>
  );
}
