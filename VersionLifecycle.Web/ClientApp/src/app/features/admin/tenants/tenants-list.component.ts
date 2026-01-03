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
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="flex justify-between items-center mb-8">
        <h1 class="text-3xl font-bold text-gray-900">Tenant Management</h1>
        <button
          (click)="onCreate()"
          class="px-4 py-2 bg-linear-to-r from-purple-600 to-indigo-600 text-white rounded-lg hover:from-purple-700 hover:to-indigo-700 transition-all shadow-md font-medium"
        >
          + Create Tenant
        </button>
      </div>

      @if (error()) {
        <div class="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700">
          {{ error() }}
        </div>
      }

      @if (loading()) {
        <div class="flex items-center justify-center py-12">
          <div class="text-center">
            <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-purple-600 mb-4"></div>
            <p class="text-gray-600">Loading tenants...</p>
          </div>
        </div>
      }

      @if (!loading()) {
        @if (filteredTenants().length === 0) {
          <div class="text-center py-16 bg-white rounded-xl shadow-sm border border-gray-200">
            <svg class="mx-auto h-16 w-16 text-gray-400 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
            </svg>
            <p class="text-gray-500 text-lg">No tenants found.</p>
            <button (click)="onCreate()" class="inline-block mt-4 text-purple-600 hover:text-purple-700 font-medium">Create your first tenant →</button>
          </div>
        }

        @if (filteredTenants().length > 0) {
          <div class="mb-6">
            <input
              type="text"
              placeholder="Search tenants..."
              class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent shadow-sm"
              (input)="onSearchChange($event)"
            />
          </div>

          <div class="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
            <div class="overflow-x-auto">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Code</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Subscription</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Created</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                  </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                  @for (tenant of filteredTenants(); track tenant.id) {
                    <tr class="hover:bg-gray-50 transition-colors">
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
                      <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                        <button
                          (click)="onViewStats(tenant.id)"
                          class="text-purple-600 hover:text-purple-900 mr-4 transition-colors cursor-pointer"
                        >
                          Stats
                        </button>
                        <button
                          (click)="onEdit(tenant.id)"
                          class="text-indigo-600 hover:text-indigo-900 transition-colors cursor-pointer"
                        >
                          Edit
                        </button>
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
