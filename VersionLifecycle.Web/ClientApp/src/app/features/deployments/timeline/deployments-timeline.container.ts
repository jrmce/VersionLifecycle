import { Component, OnInit, computed, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { DeploymentsTimelineComponent } from './deployments-timeline.component';
import { ApplicationsStore } from '../../applications/applications.store';
import { DeploymentsStore } from '../deployments.store';

@Component({
  selector: 'app-deployments-timeline-container',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, DeploymentsTimelineComponent],
  templateUrl: './deployments-timeline.container.html'
})
export class DeploymentsTimelineContainerComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly appsStore = inject(ApplicationsStore);
  private readonly depsStore = inject(DeploymentsStore);

  applications = this.appsStore.applications;
  versions = this.depsStore.versions;
  environments = this.depsStore.environments;
  loading = computed(() => this.appsStore.loading() || this.depsStore.loading());
  error = computed(() => this.appsStore.error() || this.depsStore.error());
  success = computed(() => null);

  private navigateAfterCreate = false;

  // Keep effect in field initializer to avoid NG0203 during OnInit
  private navigationEffect = effect(() => {
    const dep = this.depsStore.selectedDeployment();
    if (this.navigateAfterCreate && dep) {
      this.navigateAfterCreate = false;
      this.router.navigate(['/deployments', dep.id]);
    }
  });

  ngOnInit(): void {
    this.appsStore.loadApplications(0, 100);
    this.depsStore.loadEnvironments();
  }

  onApplicationChange(applicationId: number): void {
    this.depsStore.loadVersions(applicationId);
  }

  onSubmit(deployment: { applicationId: number; versionId: number; environmentId: number }): void {
    this.navigateAfterCreate = true;
    this.depsStore.createPendingDeployment(deployment);
  }
}
