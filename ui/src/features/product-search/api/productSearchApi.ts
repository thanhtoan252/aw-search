import type { SearchFilters, SearchResultDto } from "../types";
import { API_BASE } from "../../../shared/api/apiBase";

export async function searchProducts(
  filters: SearchFilters,
  signal: AbortSignal
): Promise<SearchResultDto> {
  const params = new URLSearchParams();
  if (filters.query)       { params.set("q", filters.query); }
  if (filters.category)    { params.set("category", filters.category); }
  if (filters.color)       { params.set("color", filters.color); }
  if (filters.productLine) { params.set("productLine", filters.productLine); }
  if (filters.minPrice)    { params.set("minPrice", filters.minPrice); }
  if (filters.maxPrice)    { params.set("maxPrice", filters.maxPrice); }
  params.set("page", filters.page.toString());
  params.set("pageSize", "20");

  const res = await fetch(`${API_BASE}/api/products/search?${params}`, { signal });
  if (!res.ok) { throw new Error(`HTTP ${res.status}`); }
  return res.json() as Promise<SearchResultDto>;
}
