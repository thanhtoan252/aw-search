import { FormControl, FormGroup } from '@angular/forms';
import { ProductQuery } from './products.models';

export type ProductFiltersForm = {
  q: FormControl<string>;
  category: FormControl<ProductQuery['category']>;
  brand: FormControl<ProductQuery['brand']>;
  color: FormControl<ProductQuery['color']>;
  minPrice: FormControl<number>;
  maxPrice: FormControl<number>;
  available: FormControl<boolean>;
};

export type ProductFiltersFormGroup = FormGroup<ProductFiltersForm>;
