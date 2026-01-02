import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiTokenCreateComponent } from './api-token-create.component';
import { ApiTokensStore } from '../api-tokens.store';
import { CreateApiTokenDto } from '../../../core/models/models';

@Component({
  selector: 'app-api-token-create-container',
  standalone: true,
  imports: [ApiTokenCreateComponent],
  template: `
    <app-api-token-create
      [loading]="store.loading()"
      [error]="store.error()"
      [createdToken]="store.createdToken()"
      (create)="onCreate($event)"
      (clearToken)="onClearToken()">
    </app-api-token-create>
  `
})
export class ApiTokenCreateContainerComponent implements OnInit {
  store = inject(ApiTokensStore);
  router = inject(Router);

  ngOnInit() {
    // Clear any previously created token when entering the create page
    this.store.clearCreatedToken();
  }

  async onCreate(dto: CreateApiTokenDto) {
    await this.store.createToken(dto);
  }

  onClearToken() {
    this.store.clearCreatedToken();
  }
}
