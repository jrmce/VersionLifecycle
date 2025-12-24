import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { TenantStore } from '../../../core/stores/tenant.store';
import { TenantStatsDto } from '../../../core/models/models';

/**
 * Component for displaying tenant statistics.
 * Shows user count, application count, version count, and deployment count.
 */
@Component({
  selector: 'app-tenant-stats',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="max-w-4xl mx-auto">
      <div class="mb-6">
        <button
          (click)="onBack()"
          class="text-blue-600 hover:text-blue-800 flex items-center"
        >
          ← Back to Tenants
        </button>
      </div>

      <div class="bg-white shadow rounded-lg">
        <div class="px-6 py-4 border-b border-gray-200">
          <h2 class="text-xl font-semibold text-gray-900">Tenant Statistics</h2>
          @if (tenantStore.selectedTenant()) {
            <p class="text-sm text-gray-500 mt-1">{{ tenantStore.selectedTenant()!.name }}</p>
          }
        </div>

        @if (tenantStore.loading()) {
          <div class="p-8 text-center text-gray-500">
            <div class="inline-block animate-spin rounded-full h-8 w-8 border-4 border-gray-300 border-t-blue-600"></div>
            <p class="mt-2">Loading statistics...</p>
          </div>
        } @else if (tenantStore.error()) {
          <div class="p-6 bg-red-50 border-l-4 border-red-400 text-red-700">
            {{ tenantStore.error() }}
          </div>
        } @else if (tenantStore.stats()) {
          <div class="p-6">
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
              <div class="bg-blue-50 rounded-lg p-6">
                <div class="text-3xl font-bold text-blue-600">
                  {{ tenantStore.stats()!.userCount }}
                </div>
                <div class="text-sm text-gray-600 mt-2">Total Users</div>
              </div>

              <div class="bg-green-50 rounded-lg p-6">
                <div class="text-3xl font-bold text-green-600">
                  {{ tenantStore.stats()!.applicationCount }}
                </div>
                <div class="text-sm text-gray-600 mt-2">Applications</div>
              </div>

              <div class="bg-purple-50 rounded-lg p-6">
                <div class="text-3xl font-bold text-purple-600">
                  {{ tenantStore.stats()!.versionCount }}
                </div>
                <div class="text-sm text-gray-600 mt-2">Versions</div>
              </div>

              <div class="bg-orange-50 rounded-lg p-6">
                <div class="text-3xl font-bold text-orange-600">
                  {{ tenantStore.stats()!.deploymentCount }}
                </div>
                <div class="text-sm text-gray-600 mt-2">Deployments</div>
              </div>
            </div>
          </div>
        } @else {
          <div class="p-6 text-center text-gray-500">
            No statistics available
          </div>
        }
      </div>
    </div>
  `,
})
export class TenantStatsComponent implements OnInit {
  protected readonly tenantStore = inject(TenantStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  ngOnInit() {
    const tenantId = this.route.snapshot.paramMap.get('id');
    if (tenantId) {
      this.tenantStore.loadTenant(tenantId);
      this.tenantStore.loadStats(tenantId);
    }
  }

  onBack() {
    this.router.navigate(['/admin/tenants']);
  }
}
