import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CreateEnvironmentDto } from '../../../core/models/models';

@Component({
  selector: 'app-environment-edit',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './environment-edit.component.html'
})
export class EnvironmentEditComponent {
  loading = input<boolean>(false);
  error = input<string | null>(null);

  save = output<CreateEnvironmentDto>();
  cancel = output<void>();
  clearError = output<void>();

  name = '';
  description = '';
  order = 0;

  onSubmit(): void {
    if (!this.name.trim()) {
      return;
    }

    const dto: CreateEnvironmentDto = {
      name: this.name.trim(),
      description: this.description.trim() || undefined,
      order: this.order
    };

    this.save.emit(dto);
  }

  onCancel(): void {
    this.cancel.emit();
  }

  onClearError(): void {
    this.clearError.emit();
  }
}
