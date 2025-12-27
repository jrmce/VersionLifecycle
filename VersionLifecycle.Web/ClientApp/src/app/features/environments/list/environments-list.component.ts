import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EnvironmentDto, UpdateEnvironmentDto } from '../../../core/models/models';

@Component({
  selector: 'app-environments-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './environments-list.component.html'
})
export class EnvironmentsListComponent {
  environments = input.required<EnvironmentDto[]>();
  loading = input<boolean>(false);
  error = input<string | null>(null);

  createEnvironment = output<void>();
  updateEnvironment = output<{ id: number; dto: UpdateEnvironmentDto }>();
  deleteEnvironment = output<number>();
  clearError = output<void>();

  editingId: number | null = null;
  editName = '';
  editDescription = '';
  editOrder = 0;

  startEdit(env: EnvironmentDto): void {
    this.editingId = env.id;
    this.editName = env.name;
    this.editDescription = env.description || '';
    this.editOrder = env.order;
  }

  cancelEdit(): void {
    this.editingId = null;
    this.editName = '';
    this.editDescription = '';
    this.editOrder = 0;
  }

  saveEdit(id: number): void {
    const dto: UpdateEnvironmentDto = {
      name: this.editName,
      description: this.editDescription || undefined,
      order: this.editOrder
    };
    this.updateEnvironment.emit({ id, dto });
    this.cancelEdit();
  }

  onDeleteEnvironment(id: number): void {
    if (confirm('Are you sure you want to delete this environment?')) {
      this.deleteEnvironment.emit(id);
    }
  }

  onCreateClick(): void {
    this.createEnvironment.emit();
  }

  onClearError(): void {
    this.clearError.emit();
  }
}
