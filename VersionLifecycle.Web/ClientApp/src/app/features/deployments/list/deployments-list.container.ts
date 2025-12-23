import { Component, OnInit, computed, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DeploymentsListComponent } from './deployments-list.component';
import { DeploymentsStore } from '../deployments.store';
import { DeploymentStatus } from '../../../core/models/models';

@Component({
  selector: 'app-deployments-list-container',
  standalone: true,
  imports: [CommonModule, FormsModule, DeploymentsListComponent],
  templateUrl: './deployments-list.container.html'
})
export class DeploymentsListContainerComponent implements OnInit {
  private readonly store = inject(DeploymentsStore);

  deployments = this.store.deployments;
  loading = this.store.loading;
  error = this.store.error;
  totalCount = this.store.totalCount;
  pageSize = this.store.take;
  currentPage = computed(() => Math.max(0, (this.store.currentPage?.() ?? 1) - 1));

  selectedStatus = signal<DeploymentStatus | ''>('');

  ngOnInit(): void {
    const skip = this.store.skip?.() ?? 0;
    const take = this.store.take?.() ?? 25;
    this.store.loadDeployments({ skip, take });
  }

  onPageChange(event: { page: number; pageSize: number }): void {
    const skip = event.page * event.pageSize;
    const status = this.selectedStatus();
    this.store.loadDeployments({ skip, take: event.pageSize, status: status || undefined });
  }

  onStatusChange(status: DeploymentStatus | ''): void {
    this.selectedStatus.set(status);
    const take = this.pageSize();
    this.store.loadDeployments({ skip: 0, take, status: status || undefined });
  }
}
