import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserDto } from '../../core/services/user.service';

/**
 * Presentational component for displaying user list.
 * Pure display logic only - no store or service injection.
 */
@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
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

      @if (loading()) {
        <div class="flex items-center justify-center py-12">
          <div class="text-center">
            <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-purple-600 mb-4"></div>
            <p class="text-gray-600">Loading users...</p>
          </div>
        </div>
      }

      @if (!loading()) {
        @if (users().length === 0) {
          <div class="text-center py-16 bg-white rounded-xl shadow-sm border border-gray-200">
            <svg class="mx-auto h-16 w-16 text-gray-400 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
            </svg>
            <p class="text-gray-500 text-lg">No users found in this tenant.</p>
          </div>
        }

        @if (users().length > 0) {
          <div class="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
            <div class="overflow-x-auto">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Email</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Role</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Created</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                  </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                  @for (user of users(); track user.id) {
                    <tr class="hover:bg-gray-50 transition-colors">
                      <td class="px-6 py-4 whitespace-nowrap">
                        <div class="text-sm font-medium text-gray-900">{{ user.email }}</div>
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap">
                        @if (editingUserId === user.id) {
                          <div class="relative">
                            <select
                              [(ngModel)]="selectedRole"
                              class="w-full px-4 py-2 pr-10 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-600 focus:border-transparent transition-all appearance-none bg-white cursor-pointer"
                            >
                              <option value="Viewer">Viewer</option>
                              <option value="Manager">Manager</option>
                              <option value="Admin">Admin</option>
                            </select>
                            <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3 text-gray-500">
                              <svg class="h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                                <path fill-rule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clip-rule="evenodd" />
                              </svg>
                            </div>
                          </div>
                        } @else {
                          <span class="px-3 py-1 inline-flex text-xs leading-5 font-semibold rounded-full"
                                [class]="getRoleBadgeClass(user.role)">
                            {{ user.role }}
                          </span>
                        }
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {{ user.createdAt | date:'short' }}
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                        @if (editingUserId === user.id) {
                          <button
                            (click)="onSaveRole(user.id)"
                            class="text-green-600 hover:text-green-900 mr-3 transition-colors cursor-pointer"
                          >
                            Save
                          </button>
                          <button
                            (click)="onCancelEdit()"
                            class="text-gray-600 hover:text-gray-900 transition-colors cursor-pointer"
                          >
                            Cancel
                          </button>
                        } @else {
                          <button
                            (click)="onEditRole(user.id, user.role)"
                            class="text-indigo-600 hover:text-indigo-900 mr-4 transition-colors cursor-pointer"
                          >
                            Edit Role
                          </button>
                          <button
                            (click)="onDelete(user.id)"
                            class="text-red-600 hover:text-red-900 transition-colors cursor-pointer"
                          >
                            Remove
                          </button>
                        }
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </div>
        }
      }
    </div>
  `,
})
export class UsersListComponent {
  // Inputs from container
  users = input.required<UserDto[]>();
  loading = input<boolean>(false);
  error = input<string | null>(null);

  // Outputs to container
  updateRole = output<{ userId: string; role: string }>();
  deleteUser = output<string>();
  clearError = output<void>();

  // Local UI state for inline editing
  editingUserId: string | null = null;
  selectedRole: string = '';

  getRoleBadgeClass(role: string): string {
    const classes: Record<string, string> = {
      'Admin': 'bg-purple-100 text-purple-800',
      'Manager': 'bg-blue-100 text-blue-800',
      'Viewer': 'bg-gray-100 text-gray-800',
    };
    return classes[role] || 'bg-gray-100 text-gray-800';
  }

  onEditRole(userId: string, currentRole: string) {
    this.editingUserId = userId;
    this.selectedRole = currentRole;
  }

  onCancelEdit() {
    this.editingUserId = null;
    this.selectedRole = '';
  }

  onSaveRole(userId: string) {
    this.updateRole.emit({ userId, role: this.selectedRole });
    this.editingUserId = null;
    this.selectedRole = '';
  }

  onDelete(userId: string) {
    if (confirm('Are you sure you want to remove this user? This action cannot be undone.')) {
      this.deleteUser.emit(userId);
    }
  }

  onClearError() {
    this.clearError.emit();
  }
}
