import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TenantDto } from '../../../core/models/models';

/**
 * Presentational component for displaying tenant list.
 * Pure display logic only - no store or service injection.
 */
@Component({
  selector: 'app-tenants-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="bg-white shadow rounded-lg">
      <div class="px-6 py-4 border-b border-gray-200">
        <h2 class="text-xl font-semibold text-gray-900">Tenant Management</h2>
      </div>

      @if (loading()) {
        <div class="p-8 text-center text-gray-500">
          <div class="inline-block animate-spin rounded-full h-8 w-8 border-4 border-gray-300 border-t-blue-600"></div>
          <p class="mt-2">Loading tenants...</p>
        </div>
      } @else if (error()) {
        <div class="p-6 bg-red-50 border-l-4 border-red-400 text-red-700">
          {{ error() }}
        </div>
      } @else {
        <div class="p-6">
          <div class="mb-4 flex justify-between items-center">
            <input
              type="text"
              placeholder="Search tenants..."
              class="px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              (input)="onSearchChange($event)"
            />
            <button
              (click)="onCreate()"
              class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 cursor-pointer"
            >
              Create Tenant
            </button>
          </div>

          @if (filteredTenants().length === 0) {
            <div class="text-center py-8 text-gray-500">
              No tenants found
            </div>
          } @else {
            <div class="overflow-x-auto">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Code</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Subscription</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Created</th>
                    <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                  </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                  @for (tenant of filteredTenants(); track tenant.id) {
                    <tr class="hover:bg-gray-50">
                      <td class="px-6 py-4 whitespace-nowrap">
                        <div class="text-sm font-medium text-gray-900">{{ tenant.name }}</div>
                        <div class="text-sm text-gray-500">{{ tenant.subdomain }}</div>
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 font-mono">
                        {{ tenant.code }}
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {{ tenant.subscriptionPlan || 'Free' }}
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {{ tenant.createdAt | date:'short' }}
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <button
                          (click)="onViewStats(tenant.id)"
                          class="text-blue-600 hover:text-blue-900 mr-3 cursor-pointer"
                        >
                          Stats
                        </button>
                        <button
                          (click)="onEdit(tenant.id)"
                          class="text-indigo-600 hover:text-indigo-900 cursor-pointer"
                        >
                          Edit
                        </button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        </div>
      }
    </div>
  `,
})
export class TenantsListComponent {
  // Inputs from container
  tenants = input.required<TenantDto[]>();
  loading = input<boolean>(false);
  error = input<string | null>(null);

  // Outputs to container
  create = output<void>();
  edit = output<string>();
  viewStats = output<string>();
  searchChange = output<string>();

  // Local UI state only
  searchTerm = '';

  // Filter logic (pure function based on inputs)
  filteredTenants() {
    const tenants = this.tenants();
    if (!this.searchTerm) {
      return tenants;
    }
    const term = this.searchTerm.toLowerCase();
    return tenants.filter(
      (t) =>
        t.name.toLowerCase().includes(term) ||
        t.code.toLowerCase().includes(term) ||
        t.subdomain.toLowerCase().includes(term)
    );
  }

  onSearchChange(event: Event) {
    this.searchTerm = (event.target as HTMLInputElement).value;
    this.searchChange.emit(this.searchTerm);
  }

  onCreate() {
    this.create.emit();
  }

  onEdit(tenantId: string) {
    this.edit.emit(tenantId);
  }

  onViewStats(tenantId: string) {
    this.viewStats.emit(tenantId);
  }
}
