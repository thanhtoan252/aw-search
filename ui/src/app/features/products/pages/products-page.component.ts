import { CdkScrollable } from '@angular/cdk/scrolling';
import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { z } from 'zod';
import { Pagination, PaginationChange } from '../../../shared/components/pagination/pagination';
import { ProductCard } from '../components/product-card/product-card';
import { ProductQuery, defaultProductQuery, productQuerySchema } from '../models/products.models';
import { ProductsStore } from '../product.store';

type SummaryFilterKey = 'category' | 'color' | 'brand';
type ProductsSearchForm = FormGroup<{
  q: FormControl<string>;
  category: FormControl<ProductQuery['category']>;
  brand: FormControl<ProductQuery['brand']>;
  color: FormControl<ProductQuery['color']>;
  minPrice: FormControl<number>;
  maxPrice: FormControl<number>;
  available: FormControl<boolean>;
}>;

@Component({
  selector: 'app-products-page',
  imports: [ReactiveFormsModule, DecimalPipe, CdkScrollable, InputTextModule, ProductCard, Pagination],
  providers: [ProductsStore],
  templateUrl: './products-page.component.html',
  styleUrl: './products-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductsPageComponent {
  readonly store = inject(ProductsStore);
  readonly skeletons = Array.from({ length: 8 });

  readonly form: ProductsSearchForm = inject(NonNullableFormBuilder).group({
    q: [defaultProductQuery.q],
    category: [defaultProductQuery.category],
    brand: [defaultProductQuery.brand],
    color: [defaultProductQuery.color],
    minPrice: [defaultProductQuery.minPrice],
    maxPrice: [defaultProductQuery.maxPrice],
    available: [defaultProductQuery.available],
  });

  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private applyingRoute = false;

  constructor() {
    this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      const query = this.parseQueryParams(params);
      this.applyingRoute = true;
      this.form.patchValue(this.formValueFromQuery(query), { emitEvent: false });
      this.applyingRoute = false;
      void this.store.load(query);
    });

    this.form.valueChanges
      .pipe(debounceTime(180), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        if (this.applyingRoute) {
          return;
        }

        void this.navigateWith({ ...this.readForm(), page: 1 });
      });
  }

  goToPage(event: PaginationChange): void {
    const pageSize = event.pageSize;
    const totalPages = Math.max(1, Math.ceil(this.store.total() / pageSize));
    const page = Math.min(Math.max(1, event.page), totalPages);

    void this.navigateWith({ ...this.store.query(), page, pageSize });
  }

  applySummaryFilter(key: SummaryFilterKey, value: string): void {
    void this.navigateWith({
      ...this.store.query(),
      [key]: this.isFacetActive(key, value) ? 'all' : value,
      page: 1,
    });
  }

  clearSummaryFilters(): void {
    void this.navigateWith({
      ...this.store.query(),
      category: 'all',
      brand: 'all',
      color: 'all',
      page: 1,
    });
  }

  isFacetActive(key: SummaryFilterKey, value: string): boolean {
    return this.store.query()[key] === value;
  }

  private readForm(): ProductQuery {
    const raw = this.form.getRawValue();
    const minPrice = Math.min(raw.minPrice, raw.maxPrice);
    const maxPrice = Math.max(raw.minPrice, raw.maxPrice);

    return productQuerySchema.parse({
      ...this.store.query(),
      ...raw,
      minPrice,
      maxPrice,
    });
  }

  private formValueFromQuery(query: ProductQuery): ReturnType<ProductsSearchForm['getRawValue']> {
    return {
      q: query.q,
      category: query.category,
      brand: query.brand,
      color: query.color,
      minPrice: query.minPrice,
      maxPrice: query.maxPrice,
      available: query.available,
    };
  }

  private parseQueryParams(params: Params): ProductQuery {
    const parsed = productQuerySchema.catch(defaultProductQuery).parse(params);

    return {
      ...parsed,
      minPrice: Math.min(parsed.minPrice, parsed.maxPrice),
      maxPrice: Math.max(parsed.minPrice, parsed.maxPrice),
    };
  }

  private navigateWith(query: ProductQuery): Promise<boolean> {
    const parsed = productQuerySchema.parse(query);
    const params = this.compactQueryParams(parsed);

    return this.router.navigate([], {
      relativeTo: this.route,
      queryParams: params,
      replaceUrl: true,
    });
  }

  private compactQueryParams(query: ProductQuery): Params {
    const params: z.infer<typeof productQuerySchema> = query;

    return Object.fromEntries(
      Object.entries(params).filter(([key, value]) => {
        const defaultValue = defaultProductQuery[key as keyof ProductQuery];
        return value !== defaultValue && value !== '';
      }),
    );
  }
}
