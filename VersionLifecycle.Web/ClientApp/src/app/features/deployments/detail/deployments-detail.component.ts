import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DeploymentDto, DeploymentEventDto } from '../../../core/models/models';

@Component({
  selector: 'app-deployments-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './deployments-detail.component.html',
  styleUrls: ['./deployments-detail.component.scss']
})
export class DeploymentsDetailComponent {
  @Input() deployment: DeploymentDto | null = null;
  @Input() events: DeploymentEventDto[] = [];
  @Input() loading = false;
  @Input() error: string | null = null;
  @Input() success: string | null = null;

  @Output() confirm = new EventEmitter<void>();

  onConfirm(): void {
    if (this.deployment?.status !== 'Pending') return;
    this.confirm.emit();
  }

  getStatusColor(status: string): string {
    const colors: { [key: string]: string } = {
      'Pending': 'warning',
      'InProgress': 'info',
      'Success': 'success',
      'Failed': 'danger',
      'Cancelled': 'secondary'
    };
    return colors[status] || 'secondary';
  }
}
