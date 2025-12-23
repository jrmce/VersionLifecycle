import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApplicationDto, VersionDto, EnvironmentDto, CreateApplicationDto, UpdateApplicationDto } from '../../../core/models/models';

@Component({
  selector: 'app-applications-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink],
  templateUrl: './applications-detail.component.html',
  styleUrls: ['./applications-detail.component.scss']
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

  form: FormGroup;
  submitted = false;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', Validators.required],
      repositoryUrl: ['', [Validators.required, Validators.pattern(/^https?:\/\/.+/)]]
    });
  }

  get f() { return this.form.controls; }

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
}
