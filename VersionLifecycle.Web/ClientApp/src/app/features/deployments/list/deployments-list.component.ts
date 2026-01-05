import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HumanizeStatusPipe } from '../../../core/pipes/humanize-status.pipe';
import { DeploymentDto, DeploymentStatus } from '../../../core/models/models';
import { SelectInputComponent, DeploymentsTableComponent } from '../../../shared/components';
import type { SelectOption, TableAction } from '../../../shared/components';

@Component({
  selector: 'app-deployments-list',
  standalone: true,
  imports: [CommonModule, SelectInputComponent, DeploymentsTableComponent],
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
  
  private humanizeStatusPipe = new HumanizeStatusPipe();
  
  get statusOptions(): SelectOption[] {
    return this.statuses.map(status => ({
      label: this.humanizeStatusPipe.transform(status),
      value: status
    }));
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  get tableActions(): TableAction[] {
    return [
      {
        label: 'View',
        callback: (row: DeploymentDto) => window.location.href = `/deployments/${row.id}`,
        class: 'inline-block px-3 py-1 bg-purple-100 text-purple-700 rounded hover:bg-purple-200 transition-colors font-medium cursor-pointer'
      },
      {
        label: 'Confirm',
        callback: (row: DeploymentDto) => this.onConfirm(row.id),
        class: 'px-3 py-1 rounded bg-green-100 text-green-700 hover:bg-green-200 transition-colors font-medium cursor-pointer',
        showCondition: (row: DeploymentDto) => row.status === 'Pending'
      },
      {
        label: 'Mark Success',
        callback: (row: DeploymentDto) => this.onUpdateStatus(row.id, 'Success'),
        class: 'px-3 py-1 rounded bg-green-100 text-green-700 hover:bg-green-200 transition-colors font-medium cursor-pointer',
        showCondition: (row: DeploymentDto) => row.status === 'InProgress'
      },
      {
        label: 'Mark Failed',
        callback: (row: DeploymentDto) => this.onUpdateStatus(row.id, 'Failed'),
        class: 'px-3 py-1 rounded bg-yellow-100 text-yellow-700 hover:bg-yellow-200 transition-colors font-medium cursor-pointer',
        showCondition: (row: DeploymentDto) => row.status === 'InProgress'
      },
      {
        label: 'Cancel',
        callback: (row: DeploymentDto) => this.onUpdateStatus(row.id, 'Cancelled'),
        class: 'px-3 py-1 rounded bg-red-100 text-red-700 hover:bg-red-200 transition-colors font-medium cursor-pointer',
        showCondition: (row: DeploymentDto) => row.status === 'Pending' || row.status === 'InProgress'
      }
    ];
  }

  get formattedDeployments(): DeploymentDto[] {
    return this.deployments;
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
