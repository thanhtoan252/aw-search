import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { z } from 'zod';
import { getAppConfig } from '../../../core/config/app.config';
import {
  ProductQuery,
  ProductSearchResponse,
  productSearchResponseSchema,
} from '../models/products.models';

const backendProductSchema = z.object({
  productId: z.number(),
  name: z.string(),
  productNumber: z.string(),
  color: z.string().nullable(),
  listPrice: z.number(),
  size: z.string().nullable(),
  categoryName: z.string().nullable(),
  subcategoryName: z.string().nullable(),
  modelName: z.string().nullable(),
  description: z.string().nullable(),
  productLine: z.string().nullable(),
  isDiscontinued: z.boolean(),
});

const backendSearchResponseSchema = z.object({
  items: z.array(backendProductSchema),
  total: z.number(),
  page: z.number(),
  pageSize: z.number(),
});

@Injectable({ providedIn: 'root' })
export class ProductsApiService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = getAppConfig().apiUrl;

  searchProducts(query: ProductQuery): Observable<ProductSearchResponse> {
    return this.http
      .get<unknown>(`${this.apiUrl}/api/products/search`, {
        params: new HttpParams({ fromObject: this.toBackendHttpParams(query) }),
      })
      .pipe(
        map((response) => this.fromBackendResponse(response)),
      );
  }

  private fromBackendResponse(response: unknown): ProductSearchResponse {
    const parsed = backendSearchResponseSchema.parse(response);

    return productSearchResponseSchema.parse({
      items: parsed.items.map((item) => {
        const model = item.modelName ?? item.name.split(' ')[0] ?? 'AW';

        return {
          id: item.productId,
          name: item.size ? `${item.name}, ${item.size}` : item.name,
          category: item.categoryName ?? 'Products',
          brand: model,
          description: item.description ?? item.productNumber,
          price: item.listPrice,
          rating: 4.4,
          available: !item.isDiscontinued,
          imageUrl: `${this.apiUrl}/api/products/${item.productId}/thumbnail`,
          tags: [item.categoryName ?? 'Products', model],
        };
      }),
      total: parsed.total,
      page: parsed.page,
      pageSize: parsed.pageSize,
    });
  }

  private toBackendHttpParams(query: ProductQuery): Record<string, string> {
    return {
      q: query.q,
      category: query.category === 'all' ? '' : query.category,
      color: query.color === 'all' ? '' : query.color,
      productLine: query.brand === 'all' ? '' : query.brand,
      minPrice: String(query.minPrice),
      maxPrice: String(query.maxPrice),
      page: String(query.page),
      pageSize: String(query.pageSize),
    };
  }

}
