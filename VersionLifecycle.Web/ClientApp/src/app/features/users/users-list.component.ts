import { Component, TemplateRef, ViewChild, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserDto } from '../../core/services/user.service';
import { DataTableComponent, SelectInputComponent } from '../../shared/components';
import type { TableColumn, TableAction, SelectOption } from '../../shared/components';

/**
 * Presentational component for displaying user list.
 * Pure display logic only - no store or service injection.
 * Uses DataTableComponent for consistent styling and simplified UX.
 */
@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule, FormsModule, DataTableComponent, SelectInputComponent],
  template: `
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="flex justify-between items-center mb-8">
        <h1 class="text-3xl font-bold text-gray-900">User Management</h1>
      </div>

      @if (error()) {
        <div class="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 flex justify-between items-center">
          <span>{{ error() }}</span>
          <button
            (click)="onClearError()"
            class="text-red-700 hover:text-red-900"
          >
            ✕
          </button>
        </div>
      }

      <ng-template #roleTemplate let-user>
        <span class="px-3 py-1 inline-flex text-xs leading-5 font-semibold rounded-full" [class]="getRoleBadgeClass(user.role)">
          {{ user.role }}
        </span>
      </ng-template>

      <app-data-table
        [columns]="tableColumns"
        [data]="formattedUsers"
        [actions]="tableActions"
        [loading]="loading()"
        [showPagination]="false"
        [emptyMessage]="'No users found in this tenant.'"
        [emptyStateIcon]="'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z'"
      />

      @if (isRoleModalOpen) {
        <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 px-4">
          <div class="bg-white rounded-xl shadow-lg w-full max-w-md p-6 space-y-6">
            <div class="flex justify-between items-center">
              <h2 class="text-xl font-semibold text-gray-900">Change Role</h2>
              <button (click)="closeRoleModal()" class="text-gray-500 hover:text-gray-700">✕</button>
            </div>

            <div class="space-y-4">
              <div>
                <p class="text-sm text-gray-600">User</p>
                <p class="text-base font-medium text-gray-900">{{ roleModalUser?.email }}</p>
              </div>

              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">Role</label>
                <app-select-input
                  id="role-select"
                  label=""
                  [options]="roleOptions"
                  [value]="selectedRole"
                  (valueChange)="selectedRole = $event"
                  placeholder="Select role"
                />
              </div>
            </div>

            <div class="flex justify-end gap-3">
              <button
                class="px-4 py-2 rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-100 transition-colors"
                (click)="closeRoleModal()"
              >
                Cancel
              </button>
              <button
                class="px-4 py-2 rounded-lg bg-purple-600 text-white hover:bg-purple-700 transition-colors"
                (click)="confirmRoleChange()"
              >
                Save
              </button>
            </div>
          </div>
        </div>
      }

      @if (isDeleteModalOpen) {
        <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 px-4">
          <div class="bg-white rounded-xl shadow-lg w-full max-w-md p-6 space-y-6">
            <div class="flex justify-between items-center">
              <h2 class="text-xl font-semibold text-gray-900">Remove User</h2>
              <button (click)="closeDeleteModal()" class="text-gray-500 hover:text-gray-700">✕</button>
            </div>

            <p class="text-gray-700">Are you sure you want to remove {{ deleteModalUser?.email }}? This action cannot be undone.</p>

            <div class="flex justify-end gap-3">
              <button
                class="px-4 py-2 rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-100 transition-colors"
                (click)="closeDeleteModal()"
              >
                Cancel
              </button>
              <button
                class="px-4 py-2 rounded-lg bg-red-600 text-white hover:bg-red-700 transition-colors"
                (click)="confirmDelete()"
              >
                Remove
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `,
})
export class UsersListComponent {
  @ViewChild('roleTemplate', { static: true }) roleTemplate!: TemplateRef<any>;

  // Inputs from container
  users = input.required<UserDto[]>();
  loading = input<boolean>(false);
  error = input<string | null>(null);

  // Outputs to container
  editRole = output<{ userId: string; role: string }>();
  deleteUser = output<string>();
  clearError = output<void>();

  // Modal state
  isRoleModalOpen = false;
  isDeleteModalOpen = false;
  roleModalUser: UserDto | null = null;
  deleteModalUser: UserDto | null = null;
  selectedRole: string = 'Viewer';

  get tableColumns(): TableColumn[] {
    return [
      { key: 'email', label: 'Email' },
      { key: 'roleBadge', label: 'Role', customTemplate: this.roleTemplate },
      { key: 'createdAtFormatted', label: 'Created' }
    ];
  }

  get tableActions(): TableAction[] {
    return [
      {
        label: 'Change Role',
        callback: (row: any) => this.openRoleModal(row),
        class: 'px-3 py-1 bg-indigo-100 text-indigo-700 rounded hover:bg-indigo-200 transition-colors font-medium cursor-pointer'
      },
      {
        label: 'Remove',
        callback: (row: any) => this.openDeleteModal(row),
        class: 'px-3 py-1 bg-red-100 text-red-700 rounded hover:bg-red-200 transition-colors font-medium cursor-pointer'
      }
    ];
  }

  get formattedUsers(): any[] {
    return this.users().map(user => ({
      ...user,
      roleBadge: user.role,
      createdAtFormatted: user.createdAt ? new Date(user.createdAt).toLocaleString('en-US', { 
        month: 'short', 
        day: 'numeric', 
        year: 'numeric',
        hour: 'numeric',
        minute: '2-digit'
      }) : '—'
    }));
  }

  getRoleBadgeClass(role: string): string {
    const classes: Record<string, string> = {
      'Admin': 'bg-purple-100 text-purple-800',
      'Manager': 'bg-blue-100 text-blue-800',
      'Viewer': 'bg-gray-100 text-gray-800',
    };
    return classes[role] || 'bg-gray-100 text-gray-800';
  }

  openRoleModal(user: UserDto) {
    this.roleModalUser = user;
    this.selectedRole = user.role;
    this.isRoleModalOpen = true;
  }

  closeRoleModal() {
    this.isRoleModalOpen = false;
    this.roleModalUser = null;
  }

  confirmRoleChange() {
    if (!this.roleModalUser) return;
    this.editRole.emit({ userId: this.roleModalUser.id, role: this.selectedRole });
    this.closeRoleModal();
  }

  get roleOptions(): SelectOption[] {
    return [
      { label: 'Viewer', value: 'Viewer' },
      { label: 'Manager', value: 'Manager' },
      { label: 'Admin', value: 'Admin' }
    ];
  }

  openDeleteModal(user: UserDto) {
    this.deleteModalUser = user;
    this.isDeleteModalOpen = true;
  }

  closeDeleteModal() {
    this.isDeleteModalOpen = false;
    this.deleteModalUser = null;
  }

  confirmDelete() {
    if (!this.deleteModalUser) return;
    this.deleteUser.emit(this.deleteModalUser.id);
    this.closeDeleteModal();
  }

  onClearError() {
    this.clearError.emit();
  }
}
