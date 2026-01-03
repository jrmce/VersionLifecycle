import { Component, OnInit, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { CreateWebhookDto, UpdateWebhookDto, WebhookDto } from '../../../core/models/models';
import { WebhooksStore } from './webhooks.store';

@Component({
  selector: 'app-webhooks',
  standalone: true,
  imports: [CommonModule, FormsModule],
  providers: [WebhooksStore],
  templateUrl: './webhooks.component.html'
})
export class WebhooksComponent implements OnInit {
  readonly store = inject(WebhooksStore);
  private route = inject(ActivatedRoute);

  newWebhook: CreateWebhookDto = {
    url: '',
    secret: '',
    events: 'deployment.completed',
    maxRetries: 5
  };

  editWebhook: UpdateWebhookDto = {};

  constructor() {
    effect(() => {
      const error = this.store.error();
      if (error) {
        console.error('Webhook error:', error);
      }
    });
  }

  ngOnInit() {
    this.route.params.subscribe(params => {
      const applicationId = params['id'];
      this.store.setApplicationId(applicationId);
      this.store.loadWebhooks(applicationId);
    });
  }

  openCreateForm() {
    this.newWebhook = {
      url: '',
      secret: '',
      events: 'deployment.completed',
      maxRetries: 5
    };
    this.store.openCreateForm();
  }

  cancelCreate() {
    this.store.closeCreateForm();
  }

  createWebhook() {
    this.store.createWebhook(this.newWebhook);
  }

  openEditForm(webhook: WebhookDto) {
    this.editWebhook = {
      url: webhook.url,
      events: webhook.events,
      isActive: webhook.isActive,
      maxRetries: webhook.maxRetries
    };
    this.store.openEditForm(webhook);
  }

  cancelEdit() {
    this.store.closeEditForm();
  }

  updateWebhook() {
    const webhook = this.store.selectedWebhook();
    if (!webhook) return;
    this.store.updateWebhook(webhook.id, this.editWebhook);
  }

  deleteWebhook(webhook: WebhookDto) {
    if (!confirm(`Are you sure you want to delete the webhook for ${webhook.url}?`)) {
      return;
    }
    this.store.deleteWebhook(webhook.id);
  }

  testWebhook(webhook: WebhookDto) {
    this.store.testWebhook(webhook);
  }

  viewEvents(webhook: WebhookDto) {
    this.store.loadWebhookEvents(webhook);
  }

  closeEventsModal() {
    this.store.closeEventsModal();
  }

  retryEvent(eventId: string) {
    this.store.retryWebhookEvent(eventId);
  }
}
