import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { WebhookDto, CreateWebhookDto, UpdateWebhookDto, WebhookEventDto } from '../models/models';
import { API_CONFIG } from './api.config';

@Injectable({
  providedIn: 'root'
})
export class WebhookService {
  constructor(private http: HttpClient) {}

  getWebhooks(applicationId: string): Observable<WebhookDto[]> {
    return this.http.get<WebhookDto[]>(
      `${API_CONFIG.apiUrl}/applications/${applicationId}/webhooks`
    );
  }

  getWebhook(applicationId: string, webhookId: string): Observable<WebhookDto> {
    return this.http.get<WebhookDto>(
      `${API_CONFIG.apiUrl}/applications/${applicationId}/webhooks/${webhookId}`
    );
  }

  createWebhook(applicationId: string, dto: CreateWebhookDto): Observable<WebhookDto> {
    return this.http.post<WebhookDto>(
      `${API_CONFIG.apiUrl}/applications/${applicationId}/webhooks`,
      dto
    );
  }

  updateWebhook(applicationId: string, webhookId: string, dto: UpdateWebhookDto): Observable<WebhookDto> {
    return this.http.put<WebhookDto>(
      `${API_CONFIG.apiUrl}/applications/${applicationId}/webhooks/${webhookId}`,
      dto
    );
  }

  deleteWebhook(applicationId: string, webhookId: string): Observable<void> {
    return this.http.delete<void>(
      `${API_CONFIG.apiUrl}/applications/${applicationId}/webhooks/${webhookId}`
    );
  }

  testWebhook(applicationId: string, webhookId: string): Observable<WebhookDto> {
    return this.http.post<WebhookDto>(
      `${API_CONFIG.apiUrl}/applications/${applicationId}/webhooks/${webhookId}/test`,
      {}
    );
  }

  getWebhookEvents(applicationId: string, webhookId: string, take: number = 50): Observable<WebhookEventDto[]> {
    return this.http.get<WebhookEventDto[]>(
      `${API_CONFIG.apiUrl}/applications/${applicationId}/webhooks/${webhookId}/events?take=${take}`
    );
  }

  retryWebhookEvent(applicationId: string, webhookId: string, eventId: string): Observable<void> {
    return this.http.post<void>(
      `${API_CONFIG.apiUrl}/applications/${applicationId}/webhooks/${webhookId}/events/${eventId}/retry`,
      {}
    );
  }
}
