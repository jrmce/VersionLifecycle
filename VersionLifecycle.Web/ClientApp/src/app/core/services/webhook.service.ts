import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { WebhookDto, CreateWebhookDto, WebhookEventDto } from '../models/models';
import { API_CONFIG } from './api.config';

@Injectable({
  providedIn: 'root'
})
export class WebhookService {
  private readonly apiUrl = `${API_CONFIG.apiUrl}/webhooks`;

  constructor(private http: HttpClient) {}

  getWebhooks(applicationId: number): Observable<WebhookDto[]> {
    return this.http.get<WebhookDto[]>(
      `${this.apiUrl}/${applicationId}/webhooks`
    );
  }

  getWebhook(applicationId: number, webhookId: number): Observable<WebhookDto> {
    return this.http.get<WebhookDto>(
      `${this.apiUrl}/${applicationId}/webhooks/${webhookId}`
    );
  }

  createWebhook(applicationId: number, dto: CreateWebhookDto): Observable<WebhookDto> {
    return this.http.post<WebhookDto>(
      `${this.apiUrl}/${applicationId}/webhooks`,
      dto
    );
  }

  deleteWebhook(applicationId: number, webhookId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${applicationId}/webhooks/${webhookId}`
    );
  }

  getWebhookEvents(applicationId: number, webhookId: number): Observable<WebhookEventDto[]> {
    return this.http.get<WebhookEventDto[]>(
      `${this.apiUrl}/${applicationId}/webhooks/${webhookId}/events`
    );
  }
}
