import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { PopoverModule } from 'primeng/popover';
import { Product, SearchExplain } from '../../models/products.models';

@Component({
  selector: 'app-product-match',
  imports: [DecimalPipe, ButtonModule, PopoverModule],
  templateUrl: './product-match.html',
  styleUrl: './product-match.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductMatch {
  readonly product = input.required<Product>();

  formatReason(detail: SearchExplain): string {
    return this.fieldLabel(detail.description);
  }

  formatSummary(explain: SearchExplain): string {
    const fields = explain.details
      .map((detail) => this.fieldLabel(detail.description))
      .filter((field, index, all) => all.indexOf(field) === index)
      .slice(0, 3);

    if (fields.length === 0) {
      return 'Matched the search terms in product text fields.';
    }

    return `Matched strongly on ${fields.join(', ')}.`;
  }

  reasonDescription(detail: SearchExplain): string {
    const field = this.fieldLabel(detail.description).toLowerCase();
    return `Search terms matched this product's ${field}.`;
  }

  private fieldLabel(description: string): string {
    const match = /(?:weight|fieldWeight)\(([^:)\s]+)[:)]/i.exec(description);
    const field = match?.[1] ?? '';

    switch (field) {
      case 'name':
        return 'Product name';
      case 'productNumber':
        return 'Product number';
      case 'modelName':
        return 'Model';
      case 'description':
        return 'Description';
      case 'categoryName':
        return 'Category';
      case 'subcategoryName':
        return 'Subcategory';
      default:
        return 'Search text';
    }
  }
}
