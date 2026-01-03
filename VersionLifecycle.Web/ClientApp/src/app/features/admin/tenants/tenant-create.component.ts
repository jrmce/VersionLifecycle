import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup } from '@angular/forms';

/**
 * Presentational component for creating a tenant.
 * Pure display logic only - no store or service injection.
 */
@Component({
  selector: 'app-tenant-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900">Create New Tenant</h1>
        <p class="mt-2 text-gray-600">Add a new tenant organization to the platform.</p>
      </div>

      <div class="max-w-2xl mx-auto">
        <div class="bg-white shadow-sm rounded-xl border border-gray-200">
          <form [formGroup]="form()" (ngSubmit)="onSubmit()">
            <div class="p-6 space-y-6">
              @if (error()) {
                <div class="p-4 bg-red-50 border border-red-200 rounded-lg text-red-700">
                  {{ error() }}
                </div>
              }

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

            <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
              <p class="text-sm text-blue-700">
                <strong>Note:</strong> A unique tenant code will be automatically generated for registration purposes.
              </p>
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
              {{ loading() ? 'Creating...' : 'Create Tenant' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
  `,
})
export class TenantCreateComponent {
  // Inputs from container
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
