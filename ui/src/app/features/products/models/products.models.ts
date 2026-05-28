import { z } from 'zod';

export const productCategorySchema = z.enum(['all', 'Bikes', 'Components', 'Accessories', 'Clothing']);
export const productBrandSchema = z.string().catch('all');
export const productColorSchema = z.enum(['all', 'Black', 'Blue', 'Grey', 'Multi', 'Red', 'Silver', 'Silver/Black', 'White', 'Yellow']);

export type SearchExplain = {
  value: number;
  description: string;
  details: SearchExplain[];
};

export const searchExplainSchema: z.ZodType<SearchExplain> = z.lazy(() =>
  z.object({
    value: z.number(),
    description: z.string(),
    details: z.array(searchExplainSchema).default([]),
  }),
);

export const productSchema = z.object({
  id: z.number(),
  name: z.string(),
  category: z.string(),
  brand: z.string(),
  description: z.string(),
  price: z.number(),
  rating: z.number().min(0).max(5),
  available: z.boolean(),
  imageUrl: z.string(),
  tags: z.array(z.string()),
  searchScore: z.number().nullable().optional(),
  matchRatio: z.number().int().min(0).max(100),
  explain: searchExplainSchema.nullable().optional(),
});

export const facetSchema = z.object({
  value: z.string(),
  count: z.number().int().min(0),
});

export const productFacetsSchema = z.object({
  categories: z.array(facetSchema).default([]),
  colors: z.array(facetSchema).default([]),
  productLines: z.array(facetSchema).default([]),
});

const queryBooleanSchema = z.preprocess((value) => {
  if (value === 'true' || value === true) {
    return true;
  }

  if (value === 'false' || value === false || value == null || value === '') {
    return false;
  }

  return value;
}, z.boolean());

export const productQuerySchema = z.object({
  q: z.string().catch(''),
  category: productCategorySchema.catch('all'),
  brand: productBrandSchema.catch('all'),
  color: productColorSchema.catch('all'),
  minPrice: z.coerce.number().min(0).max(5000).catch(0),
  maxPrice: z.coerce.number().min(0).max(5000).catch(5000),
  available: queryBooleanSchema.catch(false),
  page: z.coerce.number().int().min(1).catch(1),
  pageSize: z.coerce.number().int().min(4).max(48).catch(15),
});

export const productSearchResponseSchema = z.object({
  items: z.array(productSchema),
  total: z.number().int().min(0),
  page: z.number().int().min(1),
  pageSize: z.number().int().min(1),
  totalPages: z.number().int().min(0).default(0),
  facets: productFacetsSchema.default({
    categories: [],
    colors: [],
    productLines: [],
  }),
});

export type Product = z.infer<typeof productSchema>;
export type ProductQuery = z.infer<typeof productQuerySchema>;
export type ProductSearchResponse = z.infer<typeof productSearchResponseSchema>;
export type ProductFacets = z.infer<typeof productFacetsSchema>;

export const defaultProductQuery: ProductQuery = {
  q: '',
  category: 'all',
  brand: 'all',
  color: 'all',
  minPrice: 0,
  maxPrice: 5000,
  available: false,
  page: 1,
  pageSize: 15,
};
