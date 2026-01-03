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
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="mb-8">
        <button
          (click)="onBack()"
          class="text-purple-600 hover:text-purple-800 flex items-center mb-4 transition-colors"
        >
          ← Back to Tenants
        </button>
        <h1 class="text-3xl font-bold text-gray-900">Tenant Statistics</h1>
        @if (tenantStore.selectedTenant()) {
          <p class="mt-2 text-gray-600">{{ tenantStore.selectedTenant()!.name }}</p>
        }
      </div>

      @if (tenantStore.loading()) {
        <div class="flex items-center justify-center py-12">
          <div class="text-center">
            <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-purple-600 mb-4"></div>
            <p class="text-gray-600">Loading statistics...</p>
          </div>
        </div>
      } @else if (tenantStore.error()) {
        <div class="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700">
          {{ tenantStore.error() }}
        </div>
      } @else if (tenantStore.stats()) {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <div class="bg-blue-50 rounded-xl p-6 border border-blue-100 shadow-sm">
            <div class="text-4xl font-bold text-blue-600">
              {{ tenantStore.stats()!.userCount }}
            </div>
            <div class="text-sm text-gray-600 mt-2">Total Users</div>
          </div>

          <div class="bg-green-50 rounded-xl p-6 border border-green-100 shadow-sm">
            <div class="text-4xl font-bold text-green-600">
              {{ tenantStore.stats()!.applicationCount }}
            </div>
            <div class="text-sm text-gray-600 mt-2">Applications</div>
          </div>

          <div class="bg-purple-50 rounded-xl p-6 border border-purple-100 shadow-sm">
            <div class="text-4xl font-bold text-purple-600">
              {{ tenantStore.stats()!.versionCount }}
            </div>
            <div class="text-sm text-gray-600 mt-2">Versions</div>
          </div>

          <div class="bg-orange-50 rounded-xl p-6 border border-orange-100 shadow-sm">
            <div class="text-4xl font-bold text-orange-600">
              {{ tenantStore.stats()!.deploymentCount }}
            </div>
            <div class="text-sm text-gray-600 mt-2">Deployments</div>
          </div>
        </div>
      } @else {
        <div class="text-center py-16 bg-white rounded-xl shadow-sm border border-gray-200">
          <p class="text-gray-500 text-lg">No statistics available</p>
        </div>
      }
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
