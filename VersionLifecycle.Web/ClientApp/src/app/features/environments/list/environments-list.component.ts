import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EnvironmentDto, UpdateEnvironmentDto, CreateEnvironmentDto } from '../../../core/models/models';
import { DataTableComponent } from '../../../shared/components';
import { EditEnvironmentModalComponent, ModalData } from '../../../shared/components/edit-environment-modal.component';
import type { TableColumn, TableAction } from '../../../shared/components';

@Component({
  selector: 'app-environments-list',
  standalone: true,
  imports: [CommonModule, DataTableComponent, EditEnvironmentModalComponent],
  templateUrl: './environments-list.component.html'
})
export class EnvironmentsListComponent {
  environments = input.required<EnvironmentDto[]>();
  loading = input<boolean>(false);
  error = input<string | null>(null);

  createEnvironment = output<CreateEnvironmentDto>();
  updateEnvironment = output<{ id: string; dto: UpdateEnvironmentDto }>();
  deleteEnvironment = output<string>();
  clearError = output<void>();

  isModalOpen = false;
  modalData: ModalData = { name: '', description: '', order: 0 };

  get tableColumns(): TableColumn[] {
    return [
      { key: 'order', label: 'Order' },
      { key: 'name', label: 'Name' },
      { key: 'description', label: 'Description' }
    ];
  }

  get tableActions(): TableAction[] {
    return [
      {
        label: 'Edit',
        callback: (row: EnvironmentDto) => this.openEditModal(row),
        class: 'px-3 py-1 bg-blue-100 text-blue-700 rounded hover:bg-blue-200 transition-colors font-medium cursor-pointer'
      },
      {
        label: 'Delete',
        callback: (row: EnvironmentDto) => this.onDeleteEnvironment(row.id),
        class: 'px-3 py-1 bg-red-100 text-red-700 rounded hover:bg-red-200 transition-colors font-medium cursor-pointer'
      }
    ];
  }

  get formattedEnvironments(): (EnvironmentDto & { description: string })[] {
    return this.environments().map(env => ({
      ...env,
      description: env.description || '—'
    }));
  }

  openEditModal(env: EnvironmentDto): void {
    this.modalData = {
      id: env.id,
      name: env.name,
      description: env.description || '',
      order: env.order
    };
    this.isModalOpen = true;
  }

  openCreateModal(): void {
    this.modalData = { name: '', description: '', order: this.environments().length };
    this.isModalOpen = true;
  }

  onSaveModal(data: ModalData): void {
    if (data.id) {
      // Update existing
      const dto: UpdateEnvironmentDto = {
        name: data.name,
        description: data.description || undefined,
        order: data.order
      };
      this.updateEnvironment.emit({ id: data.id, dto });
    } else {
      // Create new
      const dto: CreateEnvironmentDto = {
        name: data.name,
        description: data.description || undefined,
        order: data.order
      };
      this.createEnvironment.emit(dto);
    }
    this.isModalOpen = false;
  }

  onCancelModal(): void {
    this.isModalOpen = false;
  }

  onDeleteEnvironment(id: string): void {
    if (confirm('Are you sure you want to delete this environment?')) {
      this.deleteEnvironment.emit(id);
    }
  }

  onCreateClick(): void {
    this.openCreateModal();
  }

  onClearError(): void {
    this.clearError.emit();
  }

  trackById(env: any): string {
    return env.id;
  }
}

