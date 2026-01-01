import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { WebhookService } from '../../../core/services/webhook.service';
import { WebhookDto, CreateWebhookDto, UpdateWebhookDto, WebhookEventDto } from '../../../core/models/models';

interface WebhooksState {
  webhooks: WebhookDto[];
  selectedWebhook: WebhookDto | null;
  webhookEvents: WebhookEventDto[];
  loading: boolean;
  error: string | null;
  showCreateForm: boolean;
  showEditForm: boolean;
  showEventsModal: boolean;
  applicationId: number;
}

const initialState: WebhooksState = {
  webhooks: [],
  selectedWebhook: null,
  webhookEvents: [],
  loading: false,
  error: null,
  showCreateForm: false,
  showEditForm: false,
  showEventsModal: false,
  applicationId: 0,
};

export const WebhooksStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed(({ webhooks }) => ({
    hasWebhooks: computed(() => webhooks().length > 0),
  })),
  withMethods((store, webhookService = inject(WebhookService)) => ({
    setApplicationId(applicationId: number): void {
      patchState(store, { applicationId });
    },

    async loadWebhooks(applicationId: number) {
      patchState(store, { loading: true, error: null, applicationId });
      try {
        const webhooks = await firstValueFrom(webhookService.getWebhooks(applicationId));
        patchState(store, {
          webhooks,
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to load webhooks',
        });
      }
    },

    async createWebhook(dto: CreateWebhookDto) {
      patchState(store, { loading: true, error: null });
      try {
        const newWebhook = await firstValueFrom(
          webhookService.createWebhook(store.applicationId(), dto)
        );
        patchState(store, {
          webhooks: [...store.webhooks(), newWebhook],
          loading: false,
          showCreateForm: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to create webhook',
        });
      }
    },

    async updateWebhook(id: number, dto: UpdateWebhookDto) {
      patchState(store, { loading: true, error: null });
      try {
        const updatedWebhook = await firstValueFrom(
          webhookService.updateWebhook(store.applicationId(), id, dto)
        );
        const webhooks = store.webhooks().map((webhook) =>
          webhook.id === id ? updatedWebhook : webhook
        );
        patchState(store, {
          webhooks,
          loading: false,
          showEditForm: false,
          selectedWebhook: null,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to update webhook',
        });
      }
    },

    async deleteWebhook(id: number) {
      patchState(store, { loading: true, error: null });
      try {
        await firstValueFrom(webhookService.deleteWebhook(store.applicationId(), id));
        const webhooks = store.webhooks().filter((webhook) => webhook.id !== id);
        patchState(store, {
          webhooks,
          loading: false,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to delete webhook',
        });
      }
    },

    async testWebhook(webhook: WebhookDto) {
      patchState(store, { loading: true, error: null });
      try {
        await firstValueFrom(webhookService.testWebhook(store.applicationId(), webhook.id));
        patchState(store, { loading: false });
        alert('Test webhook sent successfully! Check the delivery history.');
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to send test webhook',
        });
      }
    },

    async loadWebhookEvents(webhook: WebhookDto) {
      patchState(store, { loading: true, error: null, selectedWebhook: webhook });
      try {
        const webhookEvents = await firstValueFrom(
          webhookService.getWebhookEvents(store.applicationId(), webhook.id)
        );
        patchState(store, {
          webhookEvents,
          loading: false,
          showEventsModal: true,
        });
      } catch (error: any) {
        patchState(store, {
          loading: false,
          error: error.message || 'Failed to load webhook events',
        });
      }
    },

    async retryWebhookEvent(eventId: number) {
      const webhook = store.selectedWebhook();
      if (!webhook) return;

      try {
        await firstValueFrom(
          webhookService.retryWebhookEvent(store.applicationId(), webhook.id, eventId)
        );
        alert('Retry queued successfully!');
        // Reload events
        await this.loadWebhookEvents(webhook);
      } catch (error: any) {
        patchState(store, {
          error: error.message || 'Failed to retry webhook event',
        });
      }
    },

    openCreateForm(): void {
      patchState(store, { showCreateForm: true, error: null });
    },

    closeCreateForm(): void {
      patchState(store, { showCreateForm: false });
    },

    openEditForm(webhook: WebhookDto): void {
      patchState(store, { 
        selectedWebhook: webhook,
        showEditForm: true,
        error: null 
      });
    },

    closeEditForm(): void {
      patchState(store, { 
        showEditForm: false,
        selectedWebhook: null 
      });
    },

    closeEventsModal(): void {
      patchState(store, { 
        showEventsModal: false,
        selectedWebhook: null,
        webhookEvents: [] 
      });
    },

    clearError(): void {
      patchState(store, { error: null });
    },
  }))
);
