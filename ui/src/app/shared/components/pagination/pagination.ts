import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';

@Component({
  selector: 'app-pagination',
  imports: [PaginatorModule],
  templateUrl: './pagination.html',
  styleUrl: './pagination.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Pagination {
  readonly currentPage = input.required<number>();
  readonly rows = input.required<number>();
  readonly totalRecords = input.required<number>();
  readonly pageSelected = output<number>();
  readonly first = computed(() => (this.currentPage() - 1) * this.rows());
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalRecords() / this.rows())));
  readonly rangeStart = computed(() => (this.totalRecords() === 0 ? 0 : this.first() + 1));
  readonly rangeEnd = computed(() => Math.min(this.totalRecords(), this.first() + this.rows()));

  onPageChange(event: PaginatorState): void {
    const rows = event.rows ?? this.rows();
    const first = event.first ?? 0;

    this.pageSelected.emit(Math.floor(first / rows) + 1);
  }
}
