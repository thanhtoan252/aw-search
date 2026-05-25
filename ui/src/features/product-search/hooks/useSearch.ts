import { useState, useEffect, useCallback, useRef } from "react";
import type { SearchFilters, SearchResultDto } from "../types";
import { DEFAULT_FILTERS } from "../types";
import { searchProducts } from "../api/productSearchApi";

interface UseSearchReturn {
  results: SearchResultDto | null;
  loading: boolean;
  error: string | null;
  filters: SearchFilters;
  setFilters: React.Dispatch<React.SetStateAction<SearchFilters>>;
  setPage: (page: number) => void;
}

export function useSearch(): UseSearchReturn {
  const [filters, setFilters] = useState<SearchFilters>(DEFAULT_FILTERS);
  const [results, setResults] = useState<SearchResultDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const abortRef = useRef<AbortController | null>(null);

  useEffect(() => {
    abortRef.current?.abort();
    abortRef.current = new AbortController();

    setLoading(true);
    setError(null);

    searchProducts(filters, abortRef.current.signal)
      .then((data) => {
        setResults(data);
        setLoading(false);
      })
      .catch((err: Error) => {
        if (err.name === "AbortError") { return; }
        setError("Unable to connect to the Search API. Make sure the API is running.");
        setLoading(false);
      });
  }, [filters]);

  const setPage = useCallback((page: number) => {
    setFilters((f) => ({ ...f, page }));
  }, []);

  return { results, loading, error, filters, setFilters, setPage };
}
