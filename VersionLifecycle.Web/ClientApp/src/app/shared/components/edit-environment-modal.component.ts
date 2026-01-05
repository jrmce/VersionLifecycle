import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface ModalData {
  id?: string;
  name: string;
  description: string;
  order: number;
}

@Component({
  selector: 'app-edit-environment-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    @if (isOpen) {
      <!-- Modal Overlay -->
      <div class="fixed inset-0 bg-black bg-opacity-50 z-40" (click)="onCancel()"></div>
      
      <!-- Modal Content -->
      <div class="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-lg shadow-xl max-w-md w-full p-6" (click)="$event.stopPropagation()">
          <h2 class="text-2xl font-bold text-gray-900 mb-4">
            {{ data.id ? 'Edit Environment' : 'Create Environment' }}
          </h2>
          
          <div class="space-y-4">
            <div>
              <label for="order" class="block text-sm font-medium text-gray-700 mb-1">Order</label>
              <input
                type="number"
                id="order"
                [(ngModel)]="data.order"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-600 focus:border-transparent"
                min="0"
              />
            </div>
            
            <div>
              <label for="name" class="block text-sm font-medium text-gray-700 mb-1">
                Name <span class="text-red-500">*</span>
              </label>
              <input
                type="text"
                id="name"
                [(ngModel)]="data.name"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-600 focus:border-transparent"
                placeholder="Environment name"
              />
            </div>
            
            <div>
              <label for="description" class="block text-sm font-medium text-gray-700 mb-1">Description</label>
              <textarea
                id="description"
                [(ngModel)]="data.description"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-600 focus:border-transparent"
                placeholder="Description (optional)"
                rows="3"
              ></textarea>
            </div>
          </div>
          
          <div class="mt-6 flex justify-end space-x-3">
            <button
              (click)="onCancel()"
              class="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
            >
              Cancel
            </button>
            <button
              (click)="onSave()"
              [disabled]="!data.name.trim()"
              class="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {{ data.id ? 'Save Changes' : 'Create' }}
            </button>
          </div>
        </div>
      </div>
    }
  `
})
export class EditEnvironmentModalComponent {
  @Input() isOpen = false;
  @Input() data: ModalData = { name: '', description: '', order: 0 };
  
  @Output() save = new EventEmitter<ModalData>();
  @Output() cancel = new EventEmitter<void>();

  onSave(): void {
    if (this.data.name.trim()) {
      this.save.emit(this.data);
    }
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
