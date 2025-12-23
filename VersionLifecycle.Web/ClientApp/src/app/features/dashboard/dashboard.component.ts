import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DashboardStore } from './dashboard.store';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  providers: [DashboardStore],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  store = inject(DashboardStore);

  ngOnInit(): void {
    this.store.loadDashboard();
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
