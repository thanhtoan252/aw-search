import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { SelectModule } from 'primeng/select';
import { SliderModule } from 'primeng/slider';
import { ProductFiltersFormGroup } from '../../models/products-ui.models';
import { brands, categories, colors } from '../../models/products.models';

type FilterOption<T extends string> = {
  label: string;
  value: T;
};

@Component({
  selector: 'app-product-filters',
  imports: [ReactiveFormsModule, CurrencyPipe, ButtonModule, CardModule, CheckboxModule, SelectModule, SliderModule],
  templateUrl: './product-filters.html',
  styleUrl: './product-filters.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductFilters {
  readonly form = input.required<ProductFiltersFormGroup>();
  readonly resetFilters = output<void>();

  readonly categories = categories.map((category): FilterOption<typeof category> => ({
    label: category === 'all' ? 'All categories' : category,
    value: category,
  }));
  readonly brands = brands.map((brand): FilterOption<typeof brand> => ({
    label: brand === 'all' ? 'All brands' : brand,
    value: brand,
  }));
  readonly colors = colors.map((color): FilterOption<typeof color> => ({
    label: color === 'all' ? 'All colors' : color,
    value: color,
  }));
}
