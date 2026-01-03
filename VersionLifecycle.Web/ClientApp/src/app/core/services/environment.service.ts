import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { EnvironmentDto, CreateEnvironmentDto, UpdateEnvironmentDto, EnvironmentDeploymentOverview } from '../models/models';
import { API_CONFIG } from './api.config';

@Injectable({
  providedIn: 'root'
})
export class EnvironmentService {
  private readonly apiUrl = `${API_CONFIG.apiUrl}/environments`;

  constructor(private http: HttpClient) {}

  getEnvironments(): Observable<EnvironmentDto[]> {
    return this.http.get<EnvironmentDto[]>(this.apiUrl);
  }

  getEnvironment(environmentId: string): Observable<EnvironmentDto> {
    return this.http.get<EnvironmentDto>(`${this.apiUrl}/${environmentId}`);
  }

  createEnvironment(dto: CreateEnvironmentDto): Observable<EnvironmentDto> {
    return this.http.post<EnvironmentDto>(this.apiUrl, dto);
  }

  updateEnvironment(environmentId: string, dto: UpdateEnvironmentDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${environmentId}`, dto);
  }

  deleteEnvironment(environmentId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${environmentId}`);
  }

  getEnvironmentDashboard(): Observable<EnvironmentDeploymentOverview[]> {
    return this.http.get<EnvironmentDeploymentOverview[]>(`${this.apiUrl}/dashboard`);
  }
}
