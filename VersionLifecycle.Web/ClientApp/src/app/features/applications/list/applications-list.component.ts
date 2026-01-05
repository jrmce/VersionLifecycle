import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApplicationDto } from '../../../core/models/models';
import { DataTableComponent } from '../../../shared/components';
import type { TableColumn, TableAction } from '../../../shared/components';

@Component({
  selector: 'app-applications-list',
  standalone: true,
  imports: [CommonModule, RouterLink, DataTableComponent],
  templateUrl: './applications-list.component.html',
  styleUrls: ['./applications-list.component.css']
})
export class ApplicationsListComponent {
  @Input() applications: ApplicationDto[] = [];
  @Input() loading = false;
  @Input() error: string | null = null;
  @Input() currentPage = 0; // zero-based for UI
  @Input() pageSize = 10;
  @Input() totalCount = 0;

  @Output() pageChange = new EventEmitter<{ page: number; pageSize: number }>();
  @Output() delete = new EventEmitter<string>();
  @Output() edit = new EventEmitter<string>();
  @Output() viewRepository = new EventEmitter<string>();

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize) || 1;
  }

  get tableColumns(): TableColumn[] {
    return [
      { key: 'name', label: 'Name' },
      { key: 'description', label: 'Description' },
      { key: 'repositoryUrl', label: 'Repository' },
      { key: 'createdAtFormatted', label: 'Created' }
    ];
  }

  get tableActions(): TableAction[] {
    return [
      {
        label: 'View Repository',
        callback: (row: ApplicationDto) => this.onViewRepository(row.repositoryUrl),
        class: 'px-3 py-1 bg-purple-100 text-purple-700 rounded hover:bg-purple-200 transition-colors font-medium cursor-pointer',
        showCondition: (row: ApplicationDto) => !!row.repositoryUrl
      },
      {
        label: 'Edit',
        callback: (row: ApplicationDto) => this.onEdit(row.id),
        class: 'px-3 py-1 bg-gray-100 text-gray-700 rounded hover:bg-gray-200 transition-colors font-medium cursor-pointer'
      },
      {
        label: 'Delete',
        callback: (row: ApplicationDto) => this.onDelete(row.id),
        class: 'px-3 py-1 bg-red-100 text-red-700 rounded hover:bg-red-200 transition-colors font-medium cursor-pointer'
      }
    ];
  }

  get formattedApplications(): any[] {
    return this.applications.map(app => ({
      ...app,
      description: app.description || '—',
      repositoryUrl: this.truncateUrl(app.repositoryUrl),
      createdAtFormatted: app.createdAt ? new Date(app.createdAt).toLocaleString('en-US', { 
        month: 'short', 
        day: 'numeric', 
        year: 'numeric',
        hour: 'numeric',
        minute: '2-digit'
      }) : '—'
    }));
  }

  truncateUrl(url: string): string {
    if (!url) return '—';
    if (url.length > 50) {
      return url.substring(0, 47) + '...';
    }
    return url;
  }

  onViewRepository(url: string): void {
    if (url) {
      this.viewRepository.emit(url);
    }
  }

  onEdit(id: string): void {
    this.edit.emit(id);
  }

  onDelete(id: string): void {
    this.delete.emit(id);
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

  trackById(app: any): string {
    return app.id;
  }
}
