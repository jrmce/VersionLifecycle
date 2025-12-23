import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { DeploymentsDetailComponent } from './deployments-detail.component';
import { DeploymentsStore } from '../deployments.store';

@Component({
  selector: 'app-deployments-detail-container',
  standalone: true,
  imports: [CommonModule, DeploymentsDetailComponent],
  templateUrl: './deployments-detail.container.html'
})
export class DeploymentsDetailContainerComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly store = inject(DeploymentsStore);

  deployment = this.store.selectedDeployment;
  events = this.store.events;
  loading = this.store.loading;
  error = this.store.error;
  success = signal<string | null>(null);

  private id: number | null = null;

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const rawId = params['id'];
      this.id = rawId ? Number(rawId) : null;
      if (this.id) {
        this.store.loadDeployment(this.id);
        this.store.loadDeploymentEvents(this.id);
      }
    });
  }

  onConfirm(): void {
    if (!this.id) return;
    const confirmed = confirm('Confirm this deployment?');
    if (!confirmed) return;

    this.store.confirmDeployment(this.id);
    // After confirming, refresh events
    this.store.loadDeploymentEvents(this.id);
    this.success.set('Deployment confirmed! Status updated.');
  }
}
