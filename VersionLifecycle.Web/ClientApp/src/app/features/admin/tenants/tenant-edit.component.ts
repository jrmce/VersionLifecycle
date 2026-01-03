import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup } from '@angular/forms';
import { TenantDto } from '../../../core/models/models';

/**
 * Presentational component for editing a tenant.
 * Pure display logic only - no store or service injection.
 */
@Component({
  selector: 'app-tenant-edit',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="mb-8">
        <button
          (click)="onCancel()"
          class="text-purple-600 hover:text-purple-800 flex items-center mb-4 transition-colors"
        >
          ← Back to Tenants
        </button>
        <h1 class="text-3xl font-bold text-gray-900">Edit Tenant</h1>
        @if (tenant()) {
          <p class="mt-2 text-gray-600">{{ tenant()!.name }}</p>
        }
      </div>

      @if (loading() && !tenant()) {
        <div class="flex items-center justify-center py-12">
          <div class="text-center">
            <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-purple-600 mb-4"></div>
            <p class="text-gray-600">Loading tenant...</p>
          </div>
        </div>
      }

      @if (tenant()) {
        <div class="max-w-2xl mx-auto">
          <div class="bg-white shadow-sm rounded-xl border border-gray-200">
            <form [formGroup]="form()" (ngSubmit)="onSubmit()">
              <div class="p-6 space-y-6">
                @if (error()) {
                  <div class="p-4 bg-red-50 border border-red-200 rounded-lg text-red-700">
                    {{ error() }}
                  </div>
                }

                <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
                  <div class="flex items-start">
                    <div class="ml-3">
                      <p class="text-sm text-blue-700">
                        <strong>Tenant Code:</strong> <span class="font-mono">{{ tenant()!.code }}</span>
                      </p>
                      <p class="text-xs text-blue-600 mt-1">The tenant code cannot be changed after creation.</p>
                    </div>
                  </div>
                </div>

                <div>
                  <label for="name" class="block text-sm font-medium text-gray-700 mb-1">
                    Tenant Name <span class="text-red-500">*</span>
                  </label>
                  <input
                    id="name"
                    type="text"
                    formControlName="name"
                    class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    placeholder="Enter tenant name"
                  />
                  @if (form().get('name')?.invalid && form().get('name')?.touched) {
                    <p class="mt-1 text-sm text-red-600">Tenant name is required</p>
                  }
                </div>

                <div>
                  <label for="description" class="block text-sm font-medium text-gray-700 mb-1">
                    Description
                  </label>
                  <textarea
                    id="description"
                    formControlName="description"
                    rows="3"
                    class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    placeholder="Enter tenant description (optional)"
                  ></textarea>
                </div>

                <div>
                  <label for="subscriptionPlan" class="block text-sm font-medium text-gray-700 mb-1">
                    Subscription Plan
                  </label>
                  <select
                    id="subscriptionPlan"
                    formControlName="subscriptionPlan"
                    class="w-full px-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  >
                    <option value="">Select plan (optional)</option>
                    <option value="Free">Free</option>
                    <option value="Starter">Starter</option>
                    <option value="Professional">Professional</option>
                    <option value="Enterprise">Enterprise</option>
                  </select>
                </div>

                <div class="pt-4 border-t border-gray-200">
                  <div class="text-sm text-gray-600">
                    <p><strong>Created:</strong> {{ tenant()!.createdAt | date:'medium' }}</p>
                    <p class="mt-1"><strong>Last Modified:</strong> {{ tenant()!.modifiedAt | date:'medium' }}</p>
                  </div>
                </div>
              </div>

              <div class="px-6 py-4 bg-gray-50 border-t border-gray-200 flex justify-end space-x-3 rounded-b-xl">
              <button
                type="button"
                (click)="onCancel()"
                class="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-100 focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 transition-colors"
                [disabled]="loading()"
              >
                Cancel
              </button>
              <button
                type="submit"
                class="px-4 py-2 bg-linear-to-r from-purple-600 to-indigo-600 text-white rounded-lg hover:from-purple-700 hover:to-indigo-700 focus:ring-2 focus:ring-purple-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-all shadow-md font-medium"
                [disabled]="form().invalid || loading()"
              >
                @if (loading()) {
                  <span class="inline-block animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent mr-2"></span>
                }
                {{ loading() ? 'Saving...' : 'Save Changes' }}
              </button>
            </div>
          </form>
        </div>
      </div>
      }

      @if (!loading() && !tenant()) {
        <div class="text-center py-16 bg-white rounded-xl shadow-sm border border-gray-200">
          <p class="text-gray-500 text-lg">Tenant not found</p>
        </div>
      }
    </div>
  `,
})
export class TenantEditComponent {
  // Inputs from container
  tenant = input<TenantDto | null>(null);
  form = input.required<FormGroup>();
  loading = input<boolean>(false);
  error = input<string | null>(null);

  // Outputs to container
  submit = output<void>();
  cancel = output<void>();

  onSubmit() {
    if (this.form().valid) {
      this.submit.emit();
    }
  }

  onCancel() {
    this.cancel.emit();
  }
}
