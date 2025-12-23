import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApplicationDto, VersionDto, EnvironmentDto, CreateApplicationDto, UpdateApplicationDto, CreateVersionDto, CreateEnvironmentDto } from '../../../core/models/models';

@Component({
  selector: 'app-applications-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink],
  templateUrl: './applications-detail.component.html',
  styleUrls: ['./applications-detail.component.css']
})
export class ApplicationsDetailComponent implements OnChanges {
  @Input() application: ApplicationDto | null = null;
  @Input() versions: VersionDto[] = [];
  @Input() environments: EnvironmentDto[] = [];
  @Input() loading = false;
  @Input() error: string | null = null;
  @Input() success: string | null = null;
  @Input() isNew = true;

  @Output() save = new EventEmitter<CreateApplicationDto | UpdateApplicationDto>();
  @Output() createVersion = new EventEmitter<CreateVersionDto>();
  @Output() createEnvironment = new EventEmitter<CreateEnvironmentDto>();

  form: FormGroup;
  versionForm: FormGroup;
  environmentForm: FormGroup;
  submitted = false;
  versionSubmitted = false;
  environmentSubmitted = false;
  showVersionForm = false;
  showEnvironmentForm = false;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', Validators.required],
      repositoryUrl: ['', [Validators.required, Validators.pattern(/^https?:\/\/.+/)]]
    });

    this.versionForm = this.fb.group({
      versionNumber: ['', [Validators.required, Validators.pattern(/^\d+\.\d+\.\d+$/)]],
      releaseNotes: ['', Validators.required]
    });

    this.environmentForm = this.fb.group({
      name: ['', Validators.required],
      order: [1, [Validators.required, Validators.min(1)]]
    });
  }

  get f() { return this.form.controls; }
  get vf() { return this.versionForm.controls; }
  get ef() { return this.environmentForm.controls; }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['application'] && this.application) {
      this.form.patchValue({
        name: this.application.name,
        description: this.application.description,
        repositoryUrl: this.application.repositoryUrl,
      });
    }
  }

  onSubmit(): void {
    this.submitted = true;
    this.error = '';
    this.success = '';

    if (this.form.invalid) {
      return;
    }

    this.save.emit(this.form.value);
  }

  onVersionSubmit(): void {
    this.versionSubmitted = true;
    if (this.versionForm.invalid) {
      return;
    }
    this.createVersion.emit(this.versionForm.value);
    this.versionForm.reset({ versionNumber: '', releaseNotes: '' });
    this.versionSubmitted = false;
    this.showVersionForm = false;
  }

  onEnvironmentSubmit(): void {
    this.environmentSubmitted = true;
    if (this.environmentForm.invalid) {
      return;
    }
    this.createEnvironment.emit(this.environmentForm.value);
    this.environmentForm.reset({ name: '', order: this.environments.length + 1 });
    this.environmentSubmitted = false;
    this.showEnvironmentForm = false;
  }

  toggleVersionForm(): void {
    this.showVersionForm = !this.showVersionForm;
    if (!this.showVersionForm) {
      this.versionForm.reset();
      this.versionSubmitted = false;
    }
  }

  toggleEnvironmentForm(): void {
    this.showEnvironmentForm = !this.showEnvironmentForm;
    if (!this.showEnvironmentForm) {
      this.environmentForm.reset({ name: '', order: this.environments.length + 1 });
      this.environmentSubmitted = false;
    }
  }
}
