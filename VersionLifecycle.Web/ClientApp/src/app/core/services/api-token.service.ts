import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  ApiTokenDto, 
  ApiTokenCreatedDto, 
  CreateApiTokenDto, 
  UpdateApiTokenDto 
} from '../models/models';
import { API_CONFIG } from './api.config';

@Injectable({
  providedIn: 'root'
})
export class ApiTokenService {
  private readonly apiUrl = `${API_CONFIG.apiUrl}/apitokens`;

  constructor(private http: HttpClient) {}

  getApiTokens(): Observable<ApiTokenDto[]> {
    return this.http.get<ApiTokenDto[]>(this.apiUrl);
  }

  getApiToken(id: string): Observable<ApiTokenDto> {
    return this.http.get<ApiTokenDto>(`${this.apiUrl}/${id}`);
  }

  createApiToken(dto: CreateApiTokenDto): Observable<ApiTokenCreatedDto> {
    return this.http.post<ApiTokenCreatedDto>(this.apiUrl, dto);
  }

  updateApiToken(id: string, dto: UpdateApiTokenDto): Observable<ApiTokenDto> {
    return this.http.put<ApiTokenDto>(`${this.apiUrl}/${id}`, dto);
  }

  revokeApiToken(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
