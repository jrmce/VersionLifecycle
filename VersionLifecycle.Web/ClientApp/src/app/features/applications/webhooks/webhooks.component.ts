import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { WebhookService } from '../../../core/services/webhook.service';
import { WebhookDto, CreateWebhookDto, UpdateWebhookDto, WebhookEventDto } from '../../../core/models/models';

@Component({
  selector: 'app-webhooks',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './webhooks.component.html',
  styleUrls: ['./webhooks.component.css']
})
export class WebhooksComponent implements OnInit {
  applicationId = signal<number>(0);
  webhooks = signal<WebhookDto[]>([]);
  selectedWebhook = signal<WebhookDto | null>(null);
  webhookEvents = signal<WebhookEventDto[]>([]);
  
  showCreateForm = signal(false);
  showEditForm = signal(false);
  showEventsModal = signal(false);
  loading = signal(false);
  error = signal<string | null>(null);

  newWebhook: CreateWebhookDto = {
    url: '',
    secret: '',
    events: 'deployment.completed',
    maxRetries: 5
  };

  editWebhook: UpdateWebhookDto = {};

  constructor(
    private route: ActivatedRoute,
    private webhookService: WebhookService
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.applicationId.set(+params['id']);
      this.loadWebhooks();
    });
  }

  loadWebhooks() {
    this.loading.set(true);
    this.error.set(null);
    
    this.webhookService.getWebhooks(this.applicationId()).subscribe({
      next: (webhooks) => {
        this.webhooks.set(webhooks);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load webhooks');
        console.error(err);
        this.loading.set(false);
      }
    });
  }

  openCreateForm() {
    this.newWebhook = {
      url: '',
      secret: '',
      events: 'deployment.completed',
      maxRetries: 5
    };
    this.showCreateForm.set(true);
  }

  cancelCreate() {
    this.showCreateForm.set(false);
  }

  createWebhook() {
    this.loading.set(true);
    this.error.set(null);

    this.webhookService.createWebhook(this.applicationId(), this.newWebhook).subscribe({
      next: () => {
        this.showCreateForm.set(false);
        this.loadWebhooks();
      },
      error: (err) => {
        this.error.set('Failed to create webhook');
        console.error(err);
        this.loading.set(false);
      }
    });
  }

  openEditForm(webhook: WebhookDto) {
    this.selectedWebhook.set(webhook);
    this.editWebhook = {
      url: webhook.url,
      events: webhook.events,
      isActive: webhook.isActive,
      maxRetries: webhook.maxRetries
    };
    this.showEditForm.set(true);
  }

  cancelEdit() {
    this.showEditForm.set(false);
    this.selectedWebhook.set(null);
  }

  updateWebhook() {
    const webhook = this.selectedWebhook();
    if (!webhook) return;

    this.loading.set(true);
    this.error.set(null);

    this.webhookService.updateWebhook(this.applicationId(), webhook.id, this.editWebhook).subscribe({
      next: () => {
        this.showEditForm.set(false);
        this.selectedWebhook.set(null);
        this.loadWebhooks();
      },
      error: (err) => {
        this.error.set('Failed to update webhook');
        console.error(err);
        this.loading.set(false);
      }
    });
  }

  deleteWebhook(webhook: WebhookDto) {
    if (!confirm(`Are you sure you want to delete the webhook for ${webhook.url}?`)) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.webhookService.deleteWebhook(this.applicationId(), webhook.id).subscribe({
      next: () => {
        this.loadWebhooks();
      },
      error: (err) => {
        this.error.set('Failed to delete webhook');
        console.error(err);
        this.loading.set(false);
      }
    });
  }

  testWebhook(webhook: WebhookDto) {
    this.loading.set(true);
    this.error.set(null);

    this.webhookService.testWebhook(this.applicationId(), webhook.id).subscribe({
      next: () => {
        alert('Test webhook sent successfully! Check the delivery history.');
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to send test webhook');
        console.error(err);
        this.loading.set(false);
      }
    });
  }

  viewEvents(webhook: WebhookDto) {
    this.selectedWebhook.set(webhook);
    this.loading.set(true);
    this.error.set(null);

    this.webhookService.getWebhookEvents(this.applicationId(), webhook.id).subscribe({
      next: (events) => {
        this.webhookEvents.set(events);
        this.showEventsModal.set(true);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load webhook events');
        console.error(err);
        this.loading.set(false);
      }
    });
  }

  closeEventsModal() {
    this.showEventsModal.set(false);
    this.selectedWebhook.set(null);
    this.webhookEvents.set([]);
  }

  retryEvent(event: WebhookEventDto) {
    const webhook = this.selectedWebhook();
    if (!webhook) return;

    this.webhookService.retryWebhookEvent(this.applicationId(), webhook.id, event.id).subscribe({
      next: () => {
        alert('Retry queued successfully!');
        this.viewEvents(webhook);
      },
      error: (err) => {
        this.error.set('Failed to retry webhook event');
        console.error(err);
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'sent':
        return 'success';
      case 'pending':
        return 'warning';
      case 'failed':
        return 'error';
      default:
        return '';
    }
  }
}
