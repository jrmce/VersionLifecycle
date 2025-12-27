import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HumanizeStatusPipe } from '../../../core/pipes/humanize-status.pipe';
import { DeploymentDto, DeploymentStatus } from '../../../core/models/models';

@Component({
  selector: 'app-deployments-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, HumanizeStatusPipe],
  templateUrl: './deployments-list.component.html',
  styleUrls: ['./deployments-list.component.css']
})
export class DeploymentsListComponent {
  @Input() deployments: DeploymentDto[] = [];
  @Input() loading = false;
  @Input() error: string | null = null;
  @Input() selectedStatus: DeploymentStatus | '' = '';
  @Input() currentPage = 0; // zero-based
  @Input() pageSize = 10;
  @Input() totalCount = 0;

  @Output() pageChange = new EventEmitter<{ page: number; pageSize: number }>();
  @Output() statusChange = new EventEmitter<DeploymentStatus | ''>();
  @Output() confirmDeployment = new EventEmitter<number>();
  @Output() updateStatus = new EventEmitter<{ id: number; status: DeploymentStatus }>();

  statuses: DeploymentStatus[] = ['Pending', 'InProgress', 'Success', 'Failed', 'Cancelled'];

  onStatusChange(): void {
    this.statusChange.emit(this.selectedStatus);
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

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  onPreviousPage(): void {
    if (this.currentPage > 0) {
      this.pageChange.emit({ page: this.currentPage - 1, pageSize: this.pageSize });
    }
  }

  onNextPage(): void {
    if (this.currentPage < this.totalPages - 1) {
      this.pageChange.emit({ page: this.currentPage + 1, pageSize: this.pageSize });
    }
  }

  onConfirm(id: number): void {
    this.confirmDeployment.emit(id);
  }

  onUpdateStatus(id: number, status: DeploymentStatus): void {
    this.updateStatus.emit({ id, status });
  }
}
