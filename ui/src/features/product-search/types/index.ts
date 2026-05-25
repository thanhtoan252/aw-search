export interface ProductDto {
  productId: number;
  name: string;
  productNumber: string;
  color?: string;
  listPrice: number;
  size?: string;
  productLine?: string;
  categoryName?: string;
  subcategoryName?: string;
  modelName?: string;
  isDiscontinued: boolean;
}

export interface FacetItem {
  value: string;
  count: number;
}

export interface SearchResultDto {
  items: ProductDto[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
  facets: Record<string, FacetItem[]>;
}

export interface SearchFilters {
  query: string;
  category: string;
  color: string;
  productLine: string;
  minPrice: string;
  maxPrice: string;
  page: number;
}

export const DEFAULT_FILTERS: SearchFilters = {
  query: "",
  category: "",
  color: "",
  productLine: "",
  minPrice: "",
  maxPrice: "",
  page: 1,
};
