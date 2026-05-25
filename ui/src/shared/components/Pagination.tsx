interface Props {
  page: number;
  totalPages: number;
  onChange: (page: number) => void;
}

export function Pagination({ page, totalPages, onChange }: Props) {
  const pages = buildPageNumbers(page, totalPages);

  return (
    <div className="pagination">
      <button className="page-btn" disabled={page <= 1} onClick={() => onChange(page - 1)}>
        ‹ Prev
      </button>

      <div className="page-numbers">
        {pages.map((p, i) =>
          p === "..." ? (
            <span key={`ellipsis-${i}`} className="page-ellipsis">…</span>
          ) : (
            <button
              key={p}
              className={`page-num ${p === page ? "active" : ""}`}
              onClick={() => onChange(p as number)}
            >
              {p}
            </button>
          )
        )}
      </div>

      <button className="page-btn" disabled={page >= totalPages} onClick={() => onChange(page + 1)}>
        Next ›
      </button>
    </div>
  );
}

function buildPageNumbers(current: number, total: number): (number | string)[] {
  if (total <= 7) { return Array.from({ length: total }, (_, i) => i + 1); }
  const pages: (number | string)[] = [1];
  if (current > 3) { pages.push("..."); }
  for (let p = Math.max(2, current - 1); p <= Math.min(total - 1, current + 1); p++) {
    pages.push(p);
  }
  if (current < total - 2) { pages.push("..."); }
  pages.push(total);
  return pages;
}
