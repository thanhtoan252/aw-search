import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { Product } from '../../models/products.models';
import { ProductMatch } from '../product-match/product-match';

@Component({
  selector: 'app-product-card',
  imports: [CurrencyPipe, CardModule, TagModule, ProductMatch],
  templateUrl: './product-card.html',
  styleUrl: './product-card.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductCard {
  readonly product = input.required<Product>();
}
