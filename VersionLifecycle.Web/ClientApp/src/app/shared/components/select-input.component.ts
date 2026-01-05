import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

export interface SelectOption {
  label: string;
  value: any;
}

/**
 * Reusable Select Input Component
 * 
 * A presentational component that provides a consistent styled select dropdown
 * across the application. Supports reactive forms integration via ControlValueAccessor.
 * 
 * @example
 * <app-select-input
 *   label="Status"
 *   [options]="statusOptions"
 *   [(ngModel)]="selectedStatus"
 *   placeholder="Select status"
 *   [hasError]="formSubmitted && !selectedStatus"
 *   errorMessage="Status is required"
 * />
 */
@Component({
  selector: 'app-select-input',
  standalone: true,
  imports: [CommonModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectInputComponent),
      multi: true
    }
  ],
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
          (change)="onChange($event)"
          (blur)="onTouched()"
          [class]="selectClasses"
        >
          @if (placeholder) {
            <option value="">{{ placeholder }}</option>
          }
          @for (option of options; track option.value) {
            <option [value]="option.value" [selected]="option.value === value">
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
export class SelectInputComponent implements ControlValueAccessor {
  @Input() id: string = `select-${Math.random().toString(36).substr(2, 9)}`;
  @Input() label: string = '';
  @Input() placeholder: string = '';
  @Input() options: SelectOption[] = [];
  @Input() disabled: boolean = false;
  @Input() required: boolean = false;
  @Input() hasError: boolean = false;
  @Input() errorMessage: string = '';
  @Input() helpText: string = '';

  value: any = '';

  // ControlValueAccessor implementation
  private onChangeFn: (value: any) => void = () => {};
  onTouched: () => void = () => {};

  get selectClasses(): string {
    const baseClasses = 'w-full px-4 py-2 pr-10 border rounded-lg focus:ring-2 focus:ring-purple-600 focus:border-transparent transition-all appearance-none bg-white cursor-pointer';
    const errorClasses = this.hasError ? 'border-red-500' : 'border-gray-300';
    const disabledClasses = this.disabled ? 'disabled:bg-gray-100 disabled:cursor-not-allowed opacity-50' : '';
    return `${baseClasses} ${errorClasses} ${disabledClasses}`;
  }

  onChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.value = target.value;
    this.onChangeFn(this.value);
  }

  writeValue(value: any): void {
    this.value = value || '';
  }

  registerOnChange(fn: any): void {
    this.onChangeFn = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }
}
