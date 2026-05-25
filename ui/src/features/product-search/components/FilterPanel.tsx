import type { FacetItem, SearchFilters } from "../types";
import { colorToHex } from "../../../shared/utils/colorUtils";

interface Props {
  facets?: Record<string, FacetItem[]>;
  filters: SearchFilters;
  onChange: (patch: Partial<SearchFilters>) => void;
}

const PRODUCT_LINE_LABELS: Record<string, string> = {
  R: "Road",
  M: "Mountain",
  T: "Touring",
  S: "Standard",
};

export function FilterPanel({ facets, filters, onChange }: Props) {
  const hasActiveFilters =
    filters.category || filters.color || filters.productLine || filters.minPrice || filters.maxPrice;

  return (
    <div className="filter-panel">
      <div className="filter-header">
        <span>Filters</span>
        {hasActiveFilters && (
          <button
            className="clear-filters"
            onClick={() => onChange({ category: "", color: "", productLine: "", minPrice: "", maxPrice: "" })}
          >
            Clear all
          </button>
        )}
      </div>

      <div className="filter-section">
        <h4>Price (USD)</h4>
        <div className="price-range">
          <input
            type="number"
            placeholder="Min"
            value={filters.minPrice}
            onChange={(e) => onChange({ minPrice: e.target.value })}
            min={0}
          />
          <span>—</span>
          <input
            type="number"
            placeholder="Max"
            value={filters.maxPrice}
            onChange={(e) => onChange({ maxPrice: e.target.value })}
            min={0}
          />
        </div>
      </div>

      {facets?.categories && facets.categories.length > 0 && (
        <div className="filter-section">
          <h4>Category</h4>
          <ul className="facet-list">
            {facets.categories.map((item) => (
              <li key={item.value}>
                <button
                  className={`facet-item ${filters.category === item.value ? "active" : ""}`}
                  onClick={() =>
                    onChange({ category: filters.category === item.value ? "" : item.value })
                  }
                >
                  <span>{item.value}</span>
                  <span className="facet-count">{item.count.toLocaleString()}</span>
                </button>
              </li>
            ))}
          </ul>
        </div>
      )}

      {facets?.colors && facets.colors.length > 0 && (
        <div className="filter-section">
          <h4>Color</h4>
          <div className="color-swatches">
            {facets.colors.map((item) => (
              <button
                key={item.value}
                title={`${item.value} (${item.count})`}
                className={`color-swatch ${filters.color === item.value ? "active" : ""}`}
                style={{ backgroundColor: colorToHex(item.value) }}
                onClick={() =>
                  onChange({ color: filters.color === item.value ? "" : item.value })
                }
              />
            ))}
          </div>
        </div>
      )}

      {facets?.productLines && facets.productLines.length > 0 && (
        <div className="filter-section">
          <h4>Product Line</h4>
          <ul className="facet-list">
            {facets.productLines.map((item) => (
              <li key={item.value}>
                <button
                  className={`facet-item ${filters.productLine === item.value ? "active" : ""}`}
                  onClick={() =>
                    onChange({ productLine: filters.productLine === item.value ? "" : item.value })
                  }
                >
                  <span>{PRODUCT_LINE_LABELS[item.value] ?? item.value}</span>
                  <span className="facet-count">{item.count}</span>
                </button>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}
