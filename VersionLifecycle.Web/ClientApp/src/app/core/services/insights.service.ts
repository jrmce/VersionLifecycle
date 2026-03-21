import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { InsightsQueryDto, InsightsResponseDto, InsightsStatusDto } from '../models/models';
import { API_CONFIG } from './api.config';

@Injectable({
  providedIn: 'root',
})
export class InsightsService {
  private readonly apiUrl = `${API_CONFIG.apiUrl}/insights`;

  constructor(private http: HttpClient) {}

  ask(query: InsightsQueryDto): Observable<InsightsResponseDto> {
    return this.http.post<InsightsResponseDto>(`${this.apiUrl}/ask`, query);
  }

  getStatus(): Observable<InsightsStatusDto> {
    return this.http.get<InsightsStatusDto>(`${this.apiUrl}/status`);
  }
}
