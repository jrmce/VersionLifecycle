import { Component, OnInit, computed, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ApplicationsDetailComponent } from './applications-detail.component';
import { ApplicationsStore } from '../applications.store';
import { DeploymentsStore } from '../../deployments/deployments.store';
import { CreateApplicationDto, UpdateApplicationDto } from '../../../core/models/models';

@Component({
  selector: 'app-applications-detail-container',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, ApplicationsDetailComponent],
  templateUrl: './applications-detail.container.html'
})
export class ApplicationsDetailContainerComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly appsStore = inject(ApplicationsStore);
  private readonly depsStore = inject(DeploymentsStore);

  application = this.appsStore.selectedApplication;
  versions = this.depsStore.versions;
  environments = this.depsStore.environments;
  loading = computed(() => this.appsStore.loading() || this.depsStore.loading());
  error = computed(() => this.appsStore.error() || this.depsStore.error());
  success = signal<string | null>(null);
  isNew = signal<boolean>(true);

  private id: number | null = null;
  private navigateAfterCreate = signal(false);

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const rawId = params['id'];
      if (rawId && rawId !== 'new') {
        this.id = Number(rawId);
        this.isNew.set(false);
        this.appsStore.loadApplication(this.id);
        this.depsStore.loadVersions(this.id);
        this.depsStore.loadEnvironments(this.id);
      } else {
        this.isNew.set(true);
      }
    });

  }

  private autoNavigateEffect = effect(() => {
    const shouldNavigate = this.navigateAfterCreate();
    const app = this.appsStore.selectedApplication();
    if (shouldNavigate && app?.id) {
      this.navigateAfterCreate.set(false);
      this.router.navigate(['/applications', app.id]);
    }
  });

  onSave(dto: CreateApplicationDto | UpdateApplicationDto): void {
    if (this.isNew()) {
      this.navigateAfterCreate.set(true);
      this.appsStore.createApplication(dto as CreateApplicationDto);
    } else if (this.id) {
      this.appsStore.updateApplication({ id: this.id, dto: dto as UpdateApplicationDto });
      this.success.set('Application updated successfully!');
    }
  }
}
