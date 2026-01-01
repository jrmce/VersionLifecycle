import { Component, inject, OnInit } from '@angular/core';
import { ApiTokensListComponent } from './api-tokens-list.component';
import { ApiTokensStore } from '../api-tokens.store';

@Component({
  selector: 'app-api-tokens-list-container',
  standalone: true,
  imports: [ApiTokensListComponent],
  template: `
    <app-api-tokens-list
      [tokens]="store.tokens()"
      [loading]="store.loading()"
      [error]="store.error()"
      (revoke)="onRevoke($event)"
      (toggleActive)="onToggleActive($event)">
    </app-api-tokens-list>
  `
})
export class ApiTokensListContainerComponent implements OnInit {
  store = inject(ApiTokensStore);

  ngOnInit() {
    this.store.loadTokens();
  }

  async onRevoke(id: number) {
    await this.store.revokeToken(id);
    await this.store.loadTokens(); // Refresh list
  }

  async onToggleActive(event: { id: number; isActive: boolean }) {
    await this.store.updateToken(event.id, { isActive: event.isActive });
  }
}
