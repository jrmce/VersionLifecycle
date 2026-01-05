import { Component, Input, Output, EventEmitter, TemplateRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataTableComponent } from './data-table.component';
import type { TableAction, TableColumn } from './data-table.component';
import { HumanizeStatusPipe } from '../../core/pipes/humanize-status.pipe';
import { DeploymentDto } from '../../core/models/models';

@Component({
  selector: 'app-deployments-table',
  standalone: true,
  imports: [CommonModule, DataTableComponent, HumanizeStatusPipe],
  template: `
    <ng-template #statusTemplate let-deployment>
      <span [class]="getStatusClass(deployment.status)">
        {{ deployment.status | humanizeStatus }}
      </span>
    </ng-template>

    <app-data-table
      [columns]="tableColumns"
      [data]="formattedDeployments"
      [actions]="actions"
      [loading]="loading"
      [currentPage]="currentPage"
      [totalPages]="totalPages"
      [showPagination]="showPagination"
      [emptyMessage]="emptyMessage"
      [emptyStateIcon]="emptyStateIcon"
      (previousPage)="previousPage.emit()"
      (nextPage)="nextPage.emit()"
    />
  `
})
export class DeploymentsTableComponent {
  @ViewChild('statusTemplate', { static: true }) statusTemplate!: TemplateRef<any>;

  @Input() deployments: DeploymentDto[] = [];
  @Input() actions: TableAction[] = [];
  @Input() loading = false;
  @Input() currentPage = 0;
  @Input() totalPages = 1;
  @Input() showPagination = true;
  @Input() emptyMessage = 'No deployments found.';
  @Input() emptyStateIcon: string = 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2';

  @Output() previousPage = new EventEmitter<void>();
  @Output() nextPage = new EventEmitter<void>();

  private humanizeStatusPipe = new HumanizeStatusPipe();

  get tableColumns(): TableColumn[] {
    return [
      { key: 'applicationName', label: 'Application' },
      { key: 'versionDisplay', label: 'Version' },
      { key: 'environmentName', label: 'Environment' },
      { key: 'statusBadge', label: 'Status', customTemplate: this.statusTemplate },
      { key: 'deployedAtFormatted', label: 'Deployed' },
      { key: 'completedAtFormatted', label: 'Completed' }
    ];
  }

  get formattedDeployments(): any[] {
    return this.deployments.map(deployment => ({
      ...deployment,
      applicationName: deployment.applicationName || `App ${deployment.applicationId}`,
      versionDisplay: deployment.versionNumber ? `v${deployment.versionNumber}` : `v${deployment.versionId}`,
      environmentName: deployment.environmentName || `Env ${deployment.environmentId}`,
      statusBadge: deployment.status,
      deployedAtFormatted: deployment.deployedAt ? new Date(deployment.deployedAt).toLocaleString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
        hour: 'numeric',
        minute: '2-digit'
      }) : '—',
      completedAtFormatted: deployment.completedAt ? new Date(deployment.completedAt).toLocaleString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
        hour: 'numeric',
        minute: '2-digit'
      }) : '—'
    }));
  }

  getStatusClass(status: string): string {
    const base = 'px-3 py-1 rounded-full text-xs font-semibold';
    const map: Record<string, string> = {
      Success: `${base} bg-green-100 text-green-800`,
      Pending: `${base} bg-yellow-100 text-yellow-800`,
      InProgress: `${base} bg-blue-100 text-blue-800`,
      Failed: `${base} bg-red-100 text-red-800`,
      Cancelled: `${base} bg-gray-100 text-gray-800`
    };
    return map[status] || `${base} bg-gray-100 text-gray-800`;
  }
}
