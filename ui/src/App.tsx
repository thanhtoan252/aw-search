import { SearchBar, FilterPanel, ProductGrid, useSearch } from "./features/product-search";
import { IndexStatus } from "./features/indexing";
import { Pagination } from "./shared/components/Pagination";
import "./App.css";

export default function App() {
  const { results, loading, error, filters, setFilters, setPage } = useSearch();

  return (
    <div className="app">
      <header className="header">
        <div className="header-inner">
          <div className="brand">
            <h1>AdventureWorks</h1>
            <p>Product Search — powered by Elasticsearch</p>
          </div>
          <IndexStatus />
        </div>
      </header>

      <main className="main">
        <SearchBar
          value={filters.query}
          onChange={(query) => setFilters((f) => ({ ...f, query, page: 1 }))}
        />

        <div className="layout">
          <aside className="sidebar">
            <FilterPanel
              facets={results?.facets}
              filters={filters}
              onChange={(patch) => setFilters((f) => ({ ...f, ...patch, page: 1 }))}
            />
          </aside>

          <section className="content">
            {error && <div className="error-banner">⚠ {error}</div>}

            <div className="results-header">
              {!loading && (
                <span className="results-count">
                  {(results?.total ?? 0).toLocaleString()} products
                </span>
              )}
            </div>

            <ProductGrid items={results?.items ?? []} loading={loading} />

            {(results?.totalPages ?? 0) > 1 && (
              <Pagination
                page={filters.page}
                totalPages={results!.totalPages}
                onChange={setPage}
              />
            )}
          </section>
        </div>
      </main>
    </div>
  );
}
