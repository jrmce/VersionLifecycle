import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HumanizeStatusPipe } from '../../../core/pipes/humanize-status.pipe';
import { DeploymentDto, DeploymentStatus } from '../../../core/models/models';
import { SelectInputComponent } from '../../../shared/components';
import type { SelectOption } from '../../../shared/components';

@Component({
  selector: 'app-deployments-list',
  standalone: true,
  imports: [CommonModule, RouterLink, HumanizeStatusPipe, SelectInputComponent],
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
  @Output() confirmDeployment = new EventEmitter<string>();
  @Output() updateStatus = new EventEmitter<{ id: string; status: DeploymentStatus }>();

  statuses: DeploymentStatus[] = ['Pending', 'InProgress', 'Success', 'Failed', 'Cancelled'];
  
  get statusOptions(): SelectOption[] {
    return this.statuses.map(status => ({
      label: this.humanizeStatus(status),
      value: status
    }));
  }
  
  private humanizeStatus(status: string): string {
    const map: { [key: string]: string } = {
      'Pending': 'Pending',
      'InProgress': 'In Progress',
      'Success': 'Success',
      'Failed': 'Failed',
      'Cancelled': 'Cancelled'
    };
    return map[status] || status;
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  onStatusChange(value: any): void {
    this.selectedStatus = value as DeploymentStatus | '';
    this.statusChange.emit(this.selectedStatus);
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

  onConfirm(id: string): void {
    this.confirmDeployment.emit(id);
  }

  onUpdateStatus(id: string, status: DeploymentStatus): void {
    this.updateStatus.emit({ id, status });
  }
}
