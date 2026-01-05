import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface SelectOption {
  label: string;
  value: any;
}

/**
 * Reusable Select Input Component
 * 
 * A presentational component that provides a consistent styled select dropdown
 * across the application.
 * 
 * @example
 * <app-select-input
 *   label="Status"
 *   [options]="statusOptions"
 *   [value]="selectedStatus"
 *   (valueChange)="onStatusChange($event)"
 *   placeholder="Select status"
 *   [hasError]="formSubmitted && !selectedStatus"
 *   errorMessage="Status is required"
 * />
 */
@Component({
  selector: 'app-select-input',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div>
      @if (label) {
        <label [for]="id" class="block text-sm font-medium text-gray-700 mb-1">
          {{ label }}
          @if (required) {
            <span class="text-red-500">*</span>
          }
        </label>
      }
      <div class="relative">
        <select
          [id]="id"
          [disabled]="disabled"
          [value]="value"
          (change)="onSelectChange($event)"
          [class]="selectClasses"
        >
          @if (placeholder) {
            <option value="">{{ placeholder }}</option>
          }
          @for (option of options; track option.value) {
            <option [value]="option.value">
              {{ option.label }}
            </option>
          }
        </select>
        <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3 text-gray-500">
          <svg class="h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clip-rule="evenodd" />
          </svg>
        </div>
      </div>
      @if (hasError && errorMessage) {
        <div class="text-red-600 text-xs mt-1">
          {{ errorMessage }}
        </div>
      }
      @if (helpText && !hasError) {
        <p class="mt-1 text-sm text-gray-500">{{ helpText }}</p>
      }
    </div>
  `
})
export class SelectInputComponent {
  private static idCounter = 0;
  
  @Input() id: string = `select-${SelectInputComponent.idCounter++}`;
  @Input() label: string = '';
  @Input() placeholder: string = '';
  @Input() options: SelectOption[] = [];
  @Input() value: any = '';
  @Input() disabled: boolean = false;
  @Input() required: boolean = false;
  @Input() hasError: boolean = false;
  @Input() errorMessage: string = '';
  @Input() helpText: string = '';

  @Output() valueChange = new EventEmitter<any>();

  get selectClasses(): string {
    const baseClasses = 'w-full px-4 py-2 pr-10 border rounded-lg focus:ring-2 focus:ring-purple-600 focus:border-transparent transition-all appearance-none bg-white cursor-pointer';
    const errorClasses = this.hasError ? 'border-red-500' : 'border-gray-300';
    const disabledClasses = this.disabled ? 'disabled:bg-gray-100 disabled:cursor-not-allowed opacity-50' : '';
    return `${baseClasses} ${errorClasses} ${disabledClasses}`;
  }

  onSelectChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.valueChange.emit(target.value);
  }
}
