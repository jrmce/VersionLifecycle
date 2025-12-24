import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TenantLookupDto, TenantDto, CreateTenantDto, TenantStatsDto } from '../models/models';
import { API_CONFIG } from './api.config';

@Injectable({ providedIn: 'root' })
export class TenantService {
  private readonly apiUrl = `${API_CONFIG.apiUrl}/tenants`;

  constructor(private http: HttpClient) {}

  getActiveTenants(): Observable<TenantLookupDto[]> {
    return this.http.get<TenantLookupDto[]>(this.apiUrl);
  }

  getAllTenants(activeOnly: boolean = true): Observable<TenantDto[]> {
    return this.http.get<TenantDto[]>(`${this.apiUrl}?activeOnly=${activeOnly}`);
  }

  getTenant(id: string): Observable<TenantDto> {
    return this.http.get<TenantDto>(`${this.apiUrl}/${id}`);
  }

  createTenant(dto: CreateTenantDto): Observable<TenantDto> {
    return this.http.post<TenantDto>(this.apiUrl, dto);
  }

  updateTenant(id: string, dto: CreateTenantDto): Observable<TenantDto> {
    return this.http.put<TenantDto>(`${this.apiUrl}/${id}`, dto);
  }

  getTenantStats(id: string): Observable<TenantStatsDto> {
    return this.http.get<TenantStatsDto>(`${this.apiUrl}/${id}/stats`);
  }
}
