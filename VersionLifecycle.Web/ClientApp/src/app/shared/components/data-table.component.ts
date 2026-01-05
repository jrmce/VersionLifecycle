import { Component, Input, Output, EventEmitter, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface TableColumn {
  key: string;
  label: string;
  sortable?: boolean;
  customTemplate?: TemplateRef<any>;
}

export interface TableAction {
  label: string;
  callback: (row: any) => void;
  class?: string;
  showCondition?: (row: any) => boolean;
}

/**
 * Reusable Data Table Component
 * 
 * A presentational component that provides a consistent styled table
 * with pagination, loading states, and empty states.
 * 
 * @example
 * <app-data-table
 *   [columns]="columns"
 *   [data]="items"
 *   [loading]="loading()"
 *   [currentPage]="currentPage"
 *   [totalPages]="totalPages"
 *   (previousPage)="onPreviousPage()"
 *   (nextPage)="onNextPage()"
 * />
 */
@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (loading) {
      <div class="flex items-center justify-center py-12">
        <div class="text-center">
          <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-purple-600 mb-4"></div>
          <p class="text-gray-600">{{ loadingMessage }}</p>
        </div>
      </div>
    }

    @if (!loading) {
      @if (data.length === 0) {
        <div class="text-center py-16 bg-white rounded-xl shadow-sm border border-gray-200">
          <svg class="mx-auto h-16 w-16 text-gray-400 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" [attr.d]="emptyStateIcon" />
          </svg>
          <p class="text-gray-500 text-lg">{{ emptyMessage }}</p>
          @if (showEmptyAction) {
            <button
              (click)="emptyAction.emit()"
              class="inline-block mt-4 text-purple-600 hover:text-purple-700 font-medium"
            >
              {{ emptyActionLabel || 'Take Action' }} →
            </button>
          }
        </div>
      }

      @if (data.length > 0) {
        <div class="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  @for (column of columns; track column.key) {
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      {{ column.label }}
                    </th>
                  }
                  @if (actions && actions.length > 0) {
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Actions
                    </th>
                  }
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                @for (row of data; track trackBy ? trackBy(row) : row) {
                  <tr class="hover:bg-gray-50 transition-colors">
                    @for (column of columns; track column.key) {
                      <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        @if (column.customTemplate) {
                          <ng-container *ngTemplateOutlet="column.customTemplate; context: { $implicit: row }"></ng-container>
                        } @else {
                          {{ getNestedValue(row, column.key) }}
                        }
                      </td>
                    }
                    @if (actions && actions.length > 0) {
                      <td class="px-6 py-4 whitespace-nowrap text-sm">
                        <div class="flex flex-wrap gap-2 items-center">
                          @for (action of actions; track action.label) {
                            @if (!action.showCondition || action.showCondition(row)) {
                              <button
                                (click)="action.callback(row)"
                                [class]="action.class || 'inline-block px-3 py-1 bg-gray-100 text-gray-700 rounded hover:bg-gray-200 transition-colors font-medium cursor-pointer'"
                              >
                                {{ action.label }}
                              </button>
                            }
                          }
                        </div>
                      </td>
                    }
                  </tr>
                }
              </tbody>
            </table>
          </div>

          @if (showPagination) {
            <div class="px-6 py-4 bg-gray-50 border-t border-gray-200 flex items-center justify-between">
              <button 
                (click)="previousPage.emit()" 
                [disabled]="currentPage === 0"
                class="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                ← Previous
              </button>
              <span class="text-sm text-gray-600">
                Page <span class="font-semibold">{{ currentPage + 1 }}</span> of <span class="font-semibold">{{ totalPages || 1 }}</span>
              </span>
              <button 
                (click)="nextPage.emit()" 
                [disabled]="currentPage >= totalPages - 1"
                class="px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                Next →
              </button>
            </div>
          }
        </div>
      }
    }
  `
})
export class DataTableComponent {
  @Input() columns: TableColumn[] = [];
  @Input() data: any[] = [];
  @Input() actions: TableAction[] = [];
  @Input() loading: boolean = false;
  @Input() loadingMessage: string = 'Loading...';
  @Input() emptyMessage: string = 'No data found.';
  @Input() emptyActionLabel: string = '';
  @Input() showEmptyAction: boolean = false;
  @Input() emptyStateIcon: string = 'M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4';
  @Input() showPagination: boolean = true;
  @Input() currentPage: number = 0;
  @Input() totalPages: number = 1;
  @Input() trackBy?: (item: any) => any;

  @Output() previousPage = new EventEmitter<void>();
  @Output() nextPage = new EventEmitter<void>();
  @Output() emptyAction = new EventEmitter<void>();

  /**
   * Safely get nested property value from object
   * @param obj The object to get the value from
   * @param path The dot-separated path to the property (e.g., 'user.name')
   */
  getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((current, prop) => current?.[prop], obj);
  }
}
