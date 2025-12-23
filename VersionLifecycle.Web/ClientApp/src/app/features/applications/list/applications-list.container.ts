import { Component, OnInit, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApplicationsListComponent } from './applications-list.component';
import { ApplicationsStore } from '../applications.store';

@Component({
  selector: 'app-applications-list-container',
  standalone: true,
  imports: [CommonModule, ApplicationsListComponent],
  templateUrl: './applications-list.container.html'
})
export class ApplicationsListContainerComponent implements OnInit {
  private readonly store = inject(ApplicationsStore);

  // Map store signals to inputs expected by presentational component
  applications = this.store.applications;
  loading = this.store.loading;
  error = this.store.error;
  totalCount = this.store.totalCount;
  pageSize = this.store.take;
  // Convert store's one-based page to zero-based for the UI list component
  currentPage = computed(() => Math.max(0, (this.store.currentPage?.() ?? 1) - 1));

  ngOnInit(): void {
    const skip = this.store.skip?.() ?? 0;
    const take = this.store.take?.() ?? 25;
    this.store.loadApplications({ skip, take });
  }

  onPageChange(event: { page: number; pageSize: number }): void {
    const skip = event.page * event.pageSize;
    this.store.loadApplications({ skip, take: event.pageSize });
  }

  onDelete(id: number): void {
    const confirmed = confirm('Are you sure you want to delete this application?');
    if (confirmed) {
      this.store.deleteApplication(id);
    }
  }
}
