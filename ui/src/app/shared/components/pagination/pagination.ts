import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';

export type PaginationChange = {
  page: number;
  pageSize: number;
};

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
  readonly rowsPerPageOptions = input<number[]>([15, 30, 45]);
  readonly paginationChanged = output<PaginationChange>();
  readonly first = computed(() => (this.currentPage() - 1) * this.rows());

  onPageChange(event: PaginatorState): void {
    const rows = event.rows ?? this.rows();
    const first = event.first ?? 0;

    this.paginationChanged.emit({
      page: Math.floor(first / rows) + 1,
      pageSize: rows,
    });
  }
}
