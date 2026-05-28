import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { Product, ProductQuery, defaultProductQuery } from './models/products.models';
import { ProductsApiService } from './services/products-api.service';

type ProductsState = {
  query: ProductQuery;
  items: Product[];
  total: number;
  loading: boolean;
  error: string | null;
};

const initialState: ProductsState = {
  query: defaultProductQuery,
  items: [],
  total: 0,
  loading: false,
  error: null,
};

export const ProductsStore = signalStore(
  withState(initialState),
  withComputed((store) => ({
    totalPages: computed(() => Math.max(1, Math.ceil(store.total() / store.query.pageSize()))),
    rangeStart: computed(() => (store.total() === 0 ? 0 : (store.query.page() - 1) * store.query.pageSize() + 1)),
    rangeEnd: computed(() => Math.min(store.total(), store.query.page() * store.query.pageSize())),
    hasFilters: computed(() => {
      const query = store.query();
      return (
        query.q.trim().length > 0 ||
        query.category !== 'all' ||
        query.brand !== 'all' ||
        query.color !== 'all' ||
        query.minPrice !== 0 ||
        query.maxPrice !== 5000 ||
        query.available
      );
    }),
  })),
  withMethods((store, api = inject(ProductsApiService)) => ({
    async load(query: ProductQuery): Promise<void> {
      patchState(store, { query, loading: true, error: null });

      try {
        const response = await firstValueFrom(api.searchProducts(query));
        patchState(store, {
          items: response.items,
          total: response.total,
          loading: false,
        });
      } catch (error) {
        patchState(store, {
          items: [],
          total: 0,
          loading: false,
          error: error instanceof Error ? error.message : 'Unable to load products.',
        });
      }
    },
  })),
);
