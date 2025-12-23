import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApplicationDto } from '../../../core/models/models';

@Component({
  selector: 'app-applications-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './applications-list.component.html',
  styleUrls: ['./applications-list.component.scss']
})
export class ApplicationsListComponent {
  @Input() applications: ApplicationDto[] = [];
  @Input() loading = false;
  @Input() error: string | null = null;
  @Input() currentPage = 0; // zero-based for UI
  @Input() pageSize = 10;
  @Input() totalCount = 0;

  @Output() pageChange = new EventEmitter<{ page: number; pageSize: number }>();
  @Output() delete = new EventEmitter<number>();

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize) || 1;
  }

  onPreviousPage(): void {
    if (this.currentPage > 0) {
      this.pageChange.emit({ page: this.currentPage - 1, pageSize: this.pageSize });
    }
  }

  onNextPage(): void {
    if (this.currentPage < this.totalPages - 1) {
      this.pageChange.emit({ page: this.currentPage + 1, pageSize: this.pageSize });
    }
  }

  onDelete(id: number): void {
    this.delete.emit(id);
  }
}
